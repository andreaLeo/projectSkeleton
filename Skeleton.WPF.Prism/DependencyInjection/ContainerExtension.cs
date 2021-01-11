using CommonServiceLocator;
using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skeleton.WPF.Prism.DependencyInjection
{
    public class ContainerExtension : IContainerExtension<IDependencyContainer>,
        IServiceLocator
    {
        public IDependencyContainer Instance { get; private set; }

        public IScopedProvider CurrentScope { get; }

        public IScopedProvider CreateScope()
        {
            throw new NotImplementedException();
        }

        public ContainerExtension(IDependencyContainer container)
        {
            Instance = container ?? throw new ArgumentNullException(nameof(container));

            ContainerLocator.SetContainerExtension(() => this);
        }

        public void FinalizeExtension()
        {
           
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => Instance.DependencyResolver.GetAllInstances(serviceType);

        public IEnumerable<TService> GetAllInstances<TService>() => Instance.DependencyResolver.GetAllInstances<TService>();

        public object GetInstance(Type serviceType) => Instance.DependencyResolver.GetInstance(serviceType);

        public object GetInstance(Type serviceType, string key) => Instance.DependencyResolver.GetInstance(serviceType, key);

        public TService GetInstance<TService>() => Instance.DependencyResolver.GetInstance<TService>();

        public TService GetInstance<TService>(string key) => Instance.DependencyResolver.GetInstance<TService>(key);

        public object GetService(Type serviceType) => Instance.DependencyResolver.GetService(serviceType);

        public bool IsRegistered(Type type) => Instance.DependencyResolver.IsRegistered(type);

        public bool IsRegistered(Type type, string name) => Instance.DependencyResolver.IsRegistered(type, name);

        public IContainerRegistry Register(Type from, Type to)
        {
            Instance.AddTransient(from, to);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Instance.Add(from, to, ServiceLifetime.Transient, name);
            return this;
        }

        public IContainerRegistry Register(Type type, Func<object> factoryMethod)
        {
            Instance.Add(type, provider => factoryMethod());
            return this;
        }

        public IContainerRegistry Register(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.Add(type, provider => factoryMethod((IContainerProvider)provider));
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Instance.AddSingleton(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Instance.Add(type, instance, name);
            return this;
        }

        public IContainerRegistry RegisterMany(Type type, params Type[] serviceTypes)
        {
            Instance.Add(serviceTypes, type);
            return this;
        }

        public IContainerRegistry RegisterManySingleton(Type type, params Type[] serviceTypes)
        {
            Instance.Add(serviceTypes, type, ServiceLifetime.Singleton);
            return this;
        }

        public IContainerRegistry RegisterScoped(Type from, Type to)
        {
            Instance.AddScoped(from, to);
            return this;
        }

        public IContainerRegistry RegisterScoped(Type type, Func<object> factoryMethod)
        {
            Instance.AddScoped(type, provider => factoryMethod());
            return this;
        }

        public IContainerRegistry RegisterScoped(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.AddScoped(type, provider => factoryMethod((IContainerProvider)provider));
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Instance.AddSingleton(from, to);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Instance.Add(from, to, ServiceLifetime.Singleton, name);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type type, Func<object> factoryMethod)
        {
            Instance.Add(type, provider => factoryMethod(), ServiceLifetime.Singleton);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type type, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.Add(type, provider => factoryMethod((IContainerProvider)provider));
            return this;
        }

        public object Resolve(Type type) => Instance.DependencyResolver.Resolve(type);

        public object Resolve(Type type, params (Type Type, object Instance)[] parameters) => Instance.DependencyResolver.Resolve(type, parameters.Select(tuple => tuple.Instance));

        public object Resolve(Type type, string name) => Instance.DependencyResolver.Resolve(type, name);

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters) => Instance.DependencyResolver.Resolve(type, name, parameters.Select(tuple => tuple.Instance));
    }
}
