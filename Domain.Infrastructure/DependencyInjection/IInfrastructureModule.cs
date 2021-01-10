using Microsoft.Extensions.DependencyInjection;

namespace Domain.Infrastructure.DependencyInjection
{
    public interface IInfrastructureModule
    {
        IServiceCollection Bind(IServiceCollection serviceCollection);
    }
}
