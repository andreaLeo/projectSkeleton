using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Reflection;
using Domain.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Skeleton.Services.Reflection
{
    public class ObjectReflectionAccessService : IObjectReflectionAccessService
    {
        private readonly ILogger _logger;


        private readonly HashSet<Type> _registeredType = new HashSet<Type>();

        private readonly ConcurrentDictionary<Type, Func<object>> _typeConstructor =
            new ConcurrentDictionary<Type, Func<object>>();

        private readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> _typePropertiesDescriptors =
            new ConcurrentDictionary<Type, PropertyDescriptorCollection>();

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, object>>> _typePropertiesWriteAccess =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, object>>>();

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>> _typePropertiesReadAccess =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, object>>>();

        public ObjectReflectionAccessService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ObjectReflectionAccessService>();
        }

          public bool GetTypeByName(string name, out Type result) => throw new NotImplementedException();

        public PropertyDescriptorCollection GetPropertiesDescriptors(Type type)
        {
            return _typePropertiesDescriptors.ContainsKey(type)
                ? _typePropertiesDescriptors[type]
                : PropertyDescriptorCollection.Empty;
        }

        public IReadOnlyDictionary<string, Func<object, object>> GetReadAccess(Type type) => _typePropertiesReadAccess[type];

        public IReadOnlyDictionary<string, Action<object, object>> GetWriteAccess(Type type) => _typePropertiesWriteAccess[type];


        public object Construct(Type type)
        {
            if (!_registeredType.Contains(type))
            {
                RegisterType(type);
            }

            return _typeConstructor[type]();
        }

         public bool Write(object target, string propertyName, object value)
        {
            bool status = false;
            Type type = target.GetType();
            if (_typePropertiesWriteAccess.ContainsKey(type))
            {
                var setters = _typePropertiesWriteAccess[type];
                if (setters.ContainsKey(propertyName))
                {
                    setters[propertyName](target, value);
                    status = true;
                }
            }

            return status;
        }

        public bool Read(object target, string propertyName, out object result)
        {
            bool status = false;
            Type type = target.GetType();
            if (_typePropertiesReadAccess.ContainsKey(type))
            {
                var getters = _typePropertiesReadAccess[type];
                if (getters.ContainsKey(propertyName))
                {
                    result = getters[propertyName](target);

                    status = true;
                }
                else
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }

            return status;
        }

        public void RegisterType(Type type)
        {
            if (type.IsAbstract
             || !type.IsClass
             || _registeredType.Contains(type))
                return;

            string name = type.Name;
            _registeredType.Add(type);

            if (!_typePropertiesReadAccess.TryAdd(type, new ConcurrentDictionary<string, Func<object, object>>())
                || !_typePropertiesWriteAccess.TryAdd(type, new ConcurrentDictionary<string, Action<object, object>>()))
            {
                return;

            }
            var sw = Stopwatch.StartNew();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(info => info.CanWrite && info.CanRead).ToArray();

            //int index = 0;

            var tmpDescriptors = new List<PropertyDescriptor>();
            Parallel.ForEach(properties, info =>
            // foreach (var info in properties)
            {
                if (info.PropertyType.IsClass)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(info.PropertyType))
                    {
                        foreach (Type genericType in info.PropertyType.GenericTypeArguments)
                            RegisterType(genericType);
                    }
                    else
                    {
                        RegisterType(info.PropertyType);
                    }

                }
                CreateExpression(type, info);
                GetPropertyAttributes(type, info, tmpDescriptors);
            });


            CreateConstructor(type);

            _typePropertiesDescriptors.TryAdd(type, new PropertyDescriptorCollection(tmpDescriptors.ToArray()));
            sw.Stop();
            _logger.LogInformation($"{type.FullName} registered ... {sw.ElapsedMilliseconds} ms ");
        }

        private void CreateConstructor(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
            {
                var newBody = Expression.New(ctor);
                _typeConstructor.TryAdd(type, Expression.Lambda<Func<object>>(newBody).Compile());
            }
        }

        private void GetPropertyAttributes(Type type, PropertyInfo info, List<PropertyDescriptor> descriptors)
        {
            var attributes = info.GetCustomAttributes(true).Cast<Attribute>().ToArray();

            descriptors.Add(new ObjectReflectionPropertyDescriptor(type,
                info,
                attributes));
        }

        private void CreateExpression(Type type, PropertyInfo info)
        {
            var get = CreateGetExpression(type, info);
            var set = CreateSetExpression(type, info);

            if (get != null && set != null)
            {
                _typePropertiesReadAccess[type][info.Name] = get.Compile();
                _typePropertiesWriteAccess[type][info.Name] = set.Compile();
            }
        }

        private Expression<Func<object, object>> CreateGetExpression(Type type, PropertyInfo property)
        {
            MethodInfo methodInfo = property.GetGetMethod(false);
            if (methodInfo == null)
                return null;
            ParameterExpression instance = Expression.Parameter(type, "this");

            MethodCallExpression getCall = Expression.Call(instance, methodInfo);

            var lambda = Expression.Lambda(Expression.Convert(getCall, typeof(object)), instance);
            var p = Expression.Parameter(typeof(object), "cast_this");
            var invoke = Expression.Invoke(lambda, Expression.Convert(p, type));
            return Expression.Lambda<Func<object, object>>(invoke, p);
        }

        private Expression<Action<object, object>> CreateSetExpression(Type type, PropertyInfo property)
        {
            MethodInfo methodInfo = property.GetSetMethod(false);
            if (methodInfo == null)
                return null;

            ParameterExpression exp = Expression.Parameter(type);
            ParameterExpression param = Expression.Parameter(property.PropertyType);
            MethodCallExpression setCall = Expression.Call(exp, methodInfo, param);
            var lambda = Expression.Lambda(setCall, exp, param);
            var instance = Expression.Parameter(typeof(object));
            var parameter = Expression.Parameter(typeof(object));
            var invoke = Expression.Invoke(lambda, Expression.Convert(instance, type), Expression.Convert(parameter, property.PropertyType));
            return Expression.Lambda<Action<object, object>>(invoke, instance, parameter);
        }

        public void RegisterTypeProvider(ISerializationTypeProvider provider)
        {
            Task.Factory.StartNew(state =>
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                while (!provider.InstanciedTypes.IsCompleted)
                {
                    if (provider.InstanciedTypes.TryTake(out Type toRegister))
                    {
                        Task.Factory.StartNew(t => RegisterType(toRegister), TaskCreationOptions.AttachedToParent);
                    }
                }

            }, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
        }

        public bool IsRegister(Type type) => _registeredType.Contains(type);

        public bool Initialize(IDependencyResolver resolver)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
