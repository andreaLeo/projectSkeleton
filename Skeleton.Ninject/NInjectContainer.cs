using CommonServiceLocator;
using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.DependencyInjection.Descriptor;
using Domain.Infrastructure.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Ninject.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
namespace Skeleton.Ninject
{
    public class NInjectContainer : IDependencyContainer
    {
        private const string DirectRegister = nameof(DirectRegister);

        private readonly Dictionary<string, List<ServiceDescriptor>> _bindingByModuleName =
            new Dictionary<string, List<ServiceDescriptor>>
            {
                [DirectRegister] = new List<ServiceDescriptor>()
            };

        private readonly IKernel _kernel = new StandardKernel(new NinjectSettings
        {
            LoadExtensions = false
        });

        /// <summary />
        public NInjectContainer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            DependencyResolver = new NInjectResolver(_kernel);
            this.Add(new[]
                {
                    typeof(IServiceProvider),
                    typeof(IServiceLocator),
                    typeof(IDependencyResolver)
                },
                DependencyResolver);
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string path = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName.Name}.dll", SearchOption.AllDirectories).FirstOrDefault();
            return !string.IsNullOrEmpty(path) ? Assembly.LoadFrom(path) : args.RequestingAssembly;
        }

        private string _processingModule;

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator() => _bindingByModuleName.Values.SelectMany(set => set).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <inheritdoc />
        public void Add(ServiceDescriptor item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            RegisterServiceDescriptor(item);
        }

        /// <inheritdoc />
        public void Clear() => _bindingByModuleName.Clear();

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item) => _bindingByModuleName.Values.Any(set => set.Contains(item));

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _bindingByModuleName[DirectRegister].CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (_kernel.CanResolve(item.ServiceType))
            {
                _kernel.Unbind(item.ServiceType);
            }

            return _bindingByModuleName[DirectRegister].Remove(item);
        }

        /// <inheritdoc />
        public int Count => _bindingByModuleName.Values.SelectMany(set => set).Count();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(ServiceDescriptor item) => _bindingByModuleName[DirectRegister].IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, ServiceDescriptor item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            RegisterServiceDescriptor(item);
            _bindingByModuleName[DirectRegister].Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (_bindingByModuleName[DirectRegister].Count < index)
                return;

            ServiceDescriptor serviceDescriptor = _bindingByModuleName[DirectRegister][index];
            Remove(serviceDescriptor);
        }

        /// <inheritdoc />
        public ServiceDescriptor this[int index]
        {
            get => _bindingByModuleName[DirectRegister][index];
            set => _bindingByModuleName[DirectRegister][index] = value;
        }

        /// <inheritdoc />
        public IDependencyContainer LoadModule<TModule>()
            where TModule : class, IInfrastructureModule, new()
        {
            string typename = typeof(TModule).Name;
            if (_bindingByModuleName.ContainsKey(typename))
                return this;

            _processingModule = typename;
            TModule module = new TModule();
            _bindingByModuleName[typename] = new List<ServiceDescriptor>();
            module.Bind(this);
            _processingModule = string.Empty;
            return this;
        }

        /// <inheritdoc />
        public IDependencyContainer UnloadModule<TModule>()
            where TModule : class, IInfrastructureModule, new()
        {
            string typename = typeof(TModule).Name;

            if (!_bindingByModuleName.TryGetValue(typename, out List<ServiceDescriptor> valTypes))
                return this;

            foreach (ServiceDescriptor bindedType in valTypes)
            {
                _kernel.Unbind(bindedType.ServiceType);
            }

            _bindingByModuleName.Remove(typename);
            return this;
        }

        /// <inheritdoc />
        public IDependencyResolver DependencyResolver { get; protected set; }

        private void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            IBindingNamedSyntax<object> syntax;
            string bindingName = null;
            bool overrideIfExist = false;

            if (serviceDescriptor is NamedServiceDescriptor namedServiceDescriptor)
            {
                if (!string.IsNullOrEmpty(namedServiceDescriptor.Name))
                    bindingName = namedServiceDescriptor.Name;
                overrideIfExist = namedServiceDescriptor.OverrideIfExist;
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                syntax = serviceDescriptor is MultipleBindingServiceDescriptor multiple
                    ? Register(multiple.ServiceTypes,
                        multiple.ImplementationInstance,
                        overrideIfExist)

                    : Register(serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationInstance,
                        overrideIfExist);
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                syntax = serviceDescriptor is MultipleBindingServiceDescriptor multiple
                    ? Register(multiple.ServiceTypes,
                        multiple.ImplementationFactory,
                        multiple.Lifetime,
                        overrideIfExist)

                    : Register(serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationFactory,
                    serviceDescriptor.Lifetime,
                    overrideIfExist);
            }
            else
            {
                syntax = serviceDescriptor is MultipleBindingServiceDescriptor multiple
                    ? Register(multiple.ServiceTypes,
                        multiple.ImplementationType,
                        multiple.Lifetime,
                        overrideIfExist)

                    : Register(serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime,
                    overrideIfExist);
            }


            if (!string.IsNullOrEmpty(bindingName))
                syntax.Named(bindingName);


            string key = !string.IsNullOrEmpty(_processingModule) ? _processingModule : DirectRegister;

            if (_bindingByModuleName.ContainsKey(key))
            {
                _bindingByModuleName[key].Add(serviceDescriptor);
            }
        }

        private IBindingNamedSyntax<object> Register(Type[] from, object instance, bool overrideIfExist = false)
        {
            foreach (Type type in from)
                UnbindIfNeeded(type, overrideIfExist);

            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel.Bind(from).ToConstant(instance);

            return syntax;
        }

        private IBindingNamedSyntax<object> Register(Type from, object instance, bool overrideIfExist = false)
        {
            UnbindIfNeeded(from, overrideIfExist);

            IBindingWhenInNamedWithOrOnSyntax<object> syntax = _kernel.Bind(from).ToConstant(instance);

            return syntax;
        }

        /// <summary />
        public IBindingNamedSyntax<object> Register(Type[] from, Type to, ServiceLifetime lifetime = ServiceLifetime.Transient, bool overrideIfExist = false)
        {
            foreach (Type type in from)
                UnbindIfNeeded(type, overrideIfExist);

            IBindingNamedWithOrOnSyntax<object> syntax = SetLifeTime(_kernel.Bind(@from).To(to), lifetime);
            return syntax;
        }

        /// <summary />
        public IBindingNamedSyntax<object> Register(Type from, Type to, ServiceLifetime lifetime = ServiceLifetime.Transient, bool overrideIfExist = false)
        {
            UnbindIfNeeded(from, overrideIfExist);

            IBindingNamedWithOrOnSyntax<object> syntax = SetLifeTime(
                from == to
                    ? _kernel.Bind(from).ToSelf()
                    : _kernel.Bind(@from).To(to), lifetime);
            return syntax;
        }

        private IBindingNamedSyntax<object> Register(Type[] from, Func<IServiceProvider, object> func, ServiceLifetime lifetime = ServiceLifetime.Transient, bool overrideIfExist = false)
        {
            foreach (Type type in from)
                UnbindIfNeeded(type, overrideIfExist);

            return SetLifeTime(_kernel.Bind(from).ToMethod(x => func(x.Kernel.Get<IServiceProvider>())), lifetime);
        }

        private IBindingNamedSyntax<object> Register(Type from, Func<IServiceProvider, object> func, ServiceLifetime lifetime = ServiceLifetime.Transient, bool overrideIfExist = false)
        {
            UnbindIfNeeded(from, overrideIfExist);

            return SetLifeTime(_kernel.Bind(from).ToMethod(x => func(x.Kernel.Get<IServiceProvider>())), lifetime);
        }

        private void UnbindIfNeeded(Type from, bool overrideIfExist)
        {
            if (_kernel.CanResolve(from) && overrideIfExist)
            {
                _kernel.Unbind(from);
            }
        }

        private static IBindingNamedWithOrOnSyntax<T> SetLifeTime<T>(IBindingInSyntax<T> binding, ServiceLifetime lifetime)
        {
            IBindingNamedWithOrOnSyntax<T> syntax = null;
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    syntax = binding.InTransientScope();
                    break;
                case ServiceLifetime.Singleton:
                    syntax = binding.InSingletonScope();
                    break;
                case ServiceLifetime.Scoped:
                    syntax = binding.InThreadScope();
                    break;
            }
            return syntax;
        }
    }
}
