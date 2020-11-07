using System;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection.Descriptor
{
    public class NamedServiceDescriptor : OverrideServiceDescriptor
    {
        public NamedServiceDescriptor(Type serviceType, object instance, string name, bool overrideIfExist = false)
            : base(serviceType, instance, overrideIfExist)
        {
            Name = name;
        }

        public NamedServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name, bool overrideIfExist = false)
            : base(serviceType, implementationType, lifetime, overrideIfExist)
        {
            Name = name;
        }

        public NamedServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, string name, bool overrideIfExist = false)
            : base(serviceType, factory, lifetime, name, overrideIfExist)
        {
            Name = name;
        }

        /// <summary />
        public string Name { get; }
    }
}
