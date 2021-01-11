using System;
using System.Collections.Generic;
using CommonServiceLocator;

namespace Domain.Infrastructure.DependencyInjection
{
    public interface IDependencyResolver : IServiceLocator, IDisposable
    {
        /// <summary />
        object Resolve(Type type, string name);
        /// <summary />
        object Resolve(Type type, params object[] parameters);
        /// <summary />
        object Resolve(Type type, string name, params object[] parameters);

        /// <summary />
        T Resolve<T>();
        /// <summary />
        T Resolve<T>(string name);

        /// <summary />
        T Resolve<T>(params object[] parameters);
        /// <summary />
        IEnumerable<object> ResolveAll(Type type);
        /// <summary />
        IEnumerable<T> ResolveAll<T>();

        /// <summary />
        bool IsRegistered(Type type);
        /// <summary />
        bool IsRegistered(Type type, string name);

        IDependencyResolver CreateScope();
    }
}
