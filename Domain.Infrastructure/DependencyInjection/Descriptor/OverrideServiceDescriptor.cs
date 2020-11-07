using System;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection.Descriptor
{
    public class OverrideServiceDescriptor : ServiceDescriptor
    {
        public OverrideServiceDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime,
            bool overrideIfExist)
            : base(serviceType, implementationType, lifetime)
        {
            OverrideIfExist = overrideIfExist;
        }

        /// <summary />
        public OverrideServiceDescriptor(Type serviceType, 
            object instance,
            bool overrideIfExist)
            : base(serviceType, instance)
        {
            OverrideIfExist = overrideIfExist;
        }

        /// <summary />
        public OverrideServiceDescriptor(
            Type serviceType,
            Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime, 
            string name,
            bool overrideIfExist)
            : base(serviceType, factory, lifetime)
        {
            OverrideIfExist = overrideIfExist;
        }
        /// <summary />
        public bool OverrideIfExist { get; set; }
    }
}
