using System;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection.Descriptor
{
    public class MultipleBindingServiceDescriptor: NamedServiceDescriptor
    {
        /// <summary />
        public Type[] ServiceTypes { get; }

        /// <summary />
        public MultipleBindingServiceDescriptor(
            Type[] serviceType,
            Type implementationType,
            ServiceLifetime lifetime,
            string name = null,
            bool overrideIfExist = false)
            : base(serviceType[0], implementationType, lifetime, name, overrideIfExist)
        {
            ServiceTypes = serviceType;
        }

        /// <summary />
        public MultipleBindingServiceDescriptor(Type[] serviceType, object instance, string name = null, bool overrideIfExist = false)
            : base(serviceType[0], instance, name, overrideIfExist)
        {
            ServiceTypes = serviceType;
        }

        /// <summary />
        public MultipleBindingServiceDescriptor(
            Type[] serviceType,
            Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime,
            string name = null,
            bool overrideIfExist = false)
            : base(serviceType[0], factory, lifetime, name, overrideIfExist)
        {
            ServiceTypes = serviceType;
        }
    }
}
