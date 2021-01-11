using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Storage.SQL;
using Microsoft.Extensions.DependencyInjection;
using Skeleton.SQL.Dapper.Builder;

namespace Skeleton.SQL.Dapper
{
    public class DapperModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) =>
            serviceCollection.AddTransient<CustomQueryBuilder>()
                .AddSingleton<ISQLWriter, DapperWriter>()
                .AddSingleton<ISQLReader, DapperReader>();
    }
}
