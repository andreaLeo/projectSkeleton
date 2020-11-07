using System;
using Domain.Infrastructure.DependencyInjection.Descriptor;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection.Extensions
{
    public static class DependencyContainerExtensions
    {
        /// <summary />
        public static IServiceCollection Add(this IServiceCollection services,
            Type[] serviceTypes,
            Type serviceImplementation,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
        {
            services.Add(new MultipleBindingServiceDescriptor(
                serviceTypes,
                serviceImplementation,
                lifetime,
                name,
                overrideIfExist));
            return services;
        }

        /// <summary />
        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
            where TService : class
            where TImplementation : class, TService
        {
            return Add(services, typeof(TService), typeof(TImplementation), lifetime, name, overrideIfExist);
        }

        /// <summary />
        public static IServiceCollection Add(this IServiceCollection services,
            Type service,
            Type implementation,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
        {
            services.Add(new NamedServiceDescriptor(
                service,
                implementation,
                lifetime,
                name,
                overrideIfExist));

            return services;
        }

        /// <summary />
        public static IServiceCollection Add<TService>(this IServiceCollection services,
            object implementationInstance,
            string name = null,
            bool overrideIfExist = false)
            where TService : class
        {
            return Add(services, typeof(TService), implementationInstance, name, overrideIfExist);
        }

        /// <summary />
        public static IServiceCollection Add(this IServiceCollection services,
            Type serviceType,
            object implementationInstance,
            string name = null,
            bool overrideIfExist = false)
        {
            services.Add(new NamedServiceDescriptor(
                serviceType,
                implementationInstance,
                name,
                overrideIfExist));

            return services;
        }

        /// <summary />
        public static IServiceCollection Add<TService>(this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
            where TService : class
        {
            return Add(services, typeof(TService), implementationFactory, lifetime, name, overrideIfExist);
        }

        /// <summary />
        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
            where TService : class
            where TImplementation : class, TService
        {
            return Add(services, typeof(TService), implementationFactory, lifetime, name, overrideIfExist);
        }

        /// <summary />
        public static IServiceCollection Add(this IServiceCollection services,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory,
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            string name = null,
            bool overrideIfExist = false)
        {
            services.Add(new NamedServiceDescriptor(
                serviceType,
                implementationFactory,
                lifetime,
                name,
                overrideIfExist));

            return services;
        }

        public static IServiceCollection Add<TService>(this IServiceCollection services,
          object implementationInstance,
          bool overrideIfExist = false)
          where TService : class
        {
            return Add(services, typeof(TService), implementationInstance, overrideIfExist);
        }

        public static IServiceCollection Add(this IServiceCollection services,
         Type serviceType,
         object implementationInstance,
         bool overrideIfExist = false)
        {
            services.Add(new OverrideServiceDescriptor(
                serviceType,
                implementationInstance,
                overrideIfExist));

            return services;
        }


        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services,
          ServiceLifetime lifetime = ServiceLifetime.Transient,
          bool overrideIfExist = false)
          where TService : class
          where TImplementation : class, TService
        {
            return Add(services, typeof(TService), typeof(TImplementation), lifetime, overrideIfExist);
        }

        public static IServiceCollection Add(this IServiceCollection services,
           Type service,
           Type implementation,
           ServiceLifetime lifetime = ServiceLifetime.Transient,
           bool overrideIfExist = false)
        {
            services.Add(new OverrideServiceDescriptor(
                service,
                implementation,
                lifetime,
                overrideIfExist));

            return services;
        }
    }
}
