using Domain.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Skeleton.NLog
{
    public class NLogModule : IInfrastructureModule
    {
        /// <inheritdoc />
        public IServiceCollection Bind(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<ILoggerFactory, NLogLoggerFactory>()
                .AddSingleton<ILoggerProvider, NLogLoggerProvider>();
        }
    }
}
