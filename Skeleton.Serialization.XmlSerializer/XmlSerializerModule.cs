using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.DependencyInjection.Extensions;
using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.XML;
using Microsoft.Extensions.DependencyInjection;

namespace Skeleton.Serialization.XmlSerializer
{
    class XmlSerializerModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) => 
            serviceCollection.Add(new[] { typeof(ISerializer), typeof(IXmlSerializer) }, typeof(XmlSerializerModule));
    }
}
