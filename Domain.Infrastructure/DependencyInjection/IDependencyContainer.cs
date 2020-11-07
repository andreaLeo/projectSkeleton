using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection
{
    public interface IDependencyContainer : IServiceCollection
    {
        /// <summary />
        IDependencyContainer LoadModule<TModule>() 
            where TModule : class, IModule, new();

        /// <summary />
        IDependencyContainer UnloadModule<TModule>() 
            where TModule : class, IModule, new();

        /// <summary />
        IDependencyResolver DependencyResolver { get; }
    }
}
