using Domain.Infrastructure.Messaging;
using RabbitMQ.Client;
using System;

namespace Skeleton.Amqp.RabbitMq.Connector.Rpc
{
      public class ResponseAction
    {
        public Action<IMessage, MessageMetadata> OnSuccess { get; set; }
        public Action OnFailure { get; set; }
        public IBasicConsumer Consumer { get; set; }
        public Type ExpectedResponse { get; set; }
        public string ResponseQueue { get; set; }
    }
}
