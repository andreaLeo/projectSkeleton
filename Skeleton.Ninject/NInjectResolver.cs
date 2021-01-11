using CommonServiceLocator;
using Domain.Infrastructure.DependencyInjection;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Parameters;
using Skeleton.Ninject.ConstructorArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skeleton.Ninject
{
    public class NInjectResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        /// <summary />
        public NInjectResolver(IKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            ServiceLocator.SetLocatorProvider(() => this);
        }

        /// <inheritdoc />
        public object GetService(Type serviceType) => _kernel.Get(serviceType);
        /// <inheritdoc />
        public object Resolve(Type serviceType, string name) => _kernel.Get(serviceType, name);
        /// <inheritdoc />
        public object Resolve(Type serviceType, params object[] parameters) => _kernel.Get(serviceType, ConvertParameters(parameters));
        /// <inheritdoc />
        public object Resolve(Type serviceType, string name, params object[] parameters) => _kernel.Get(serviceType, name, ConvertParameters(parameters));
        /// <inheritdoc />
        public T Resolve<T>() => _kernel.Get<T>();

        private IParameter[] ConvertParameters(params object[] parameters) => parameters
                .Select(p => new TypeMatchingOrAssignableCtorArg(p.GetType(), (context, target) => p))
                .Cast<IParameter>()
                .ToArray();

        /// <inheritdoc />
        public T Resolve<T>(params object[] parameters) => _kernel.Get<T>(ConvertParameters(parameters));

        /// <inheritdoc />
        public T Resolve<T>(string name) => _kernel.Get<T>(name);

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type type) => _kernel.GetAll(type);
        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>() => _kernel.GetAll<T>();
        /// <inheritdoc />
        public bool IsRegistered(Type type) => _kernel.CanResolve(type);
        /// <inheritdoc />
        public bool IsRegistered(Type type, string name) => _kernel.CanResolve(type, name);
        /// <inheritdoc />
        public object GetInstance(Type serviceType) => Resolve(serviceType);
        /// <inheritdoc />
        public object GetInstance(Type serviceType, string key) => Resolve(serviceType, key);
        /// <inheritdoc />
        public IEnumerable<object> GetAllInstances(Type serviceType) => ResolveAll(serviceType);
        /// <inheritdoc />
        public TService GetInstance<TService>() => Resolve<TService>();
        /// <inheritdoc />
        public TService GetInstance<TService>(string key) => Resolve<TService>(key);
        /// <inheritdoc />
        public IEnumerable<TService> GetAllInstances<TService>() => ResolveAll<TService>();

        public void Dispose()
        {
            if (!_kernel.IsDisposed)
            {
                _kernel.Dispose();
            }
        }

        public IDependencyResolver CreateScope() => new NInjectResolver(new ChildKernel(_kernel));
    }
}
