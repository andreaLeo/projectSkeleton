using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection
{
    public interface IModule
    {
        IServiceCollection Bind(IServiceCollection serviceCollection);
    }
}
