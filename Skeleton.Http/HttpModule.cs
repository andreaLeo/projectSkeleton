using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging.HTTP;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Http
{
    public class HttpModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) => serviceCollection.AddTransient<IHttpConnector, HttpConnector>();
    }
}
