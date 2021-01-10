using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection
{
    public interface IDependencyContainer : IServiceCollection
    {
        /// <summary />
        IDependencyContainer LoadModule<TModule>() 
            where TModule : class, IInfrastructureModule, new();

        /// <summary />
        IDependencyContainer UnloadModule<TModule>() 
            where TModule : class, IInfrastructureModule, new();

        /// <summary />
        IDependencyResolver DependencyResolver { get; }
    }
}
