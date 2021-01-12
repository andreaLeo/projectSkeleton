using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging.AMQP;
using Microsoft.Extensions.DependencyInjection;
using Skeleton.Amqp.RabbitMq.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq
{
    public class RabbitMqModule : IInfrastructureModule
    {
        public IServiceCollection Bind(IServiceCollection serviceCollection) => serviceCollection.AddSingleton<RabbitMqConnection>()
                .AddTransient<IReceiveMessageConnector, ReceiveMessageConnector>()
                .AddTransient<IRpcMessageConnector, RpcMessageConnector>()
                .AddTransient<IFireAndForget, FireAndForgetConnector>();
    }
}
