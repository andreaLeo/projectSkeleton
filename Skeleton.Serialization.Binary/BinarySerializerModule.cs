using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.DependencyInjection.Extensions;
using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.BinarySerializer;
using Microsoft.Extensions.DependencyInjection;

namespace Skeleton.Serialization.Binary
{
    public class BinarySerializerModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) =>
            serviceCollection.Add(new[] { typeof(ISerializer), typeof(IBinarySerializer) }, typeof(BinarySerializer));
    }
}
