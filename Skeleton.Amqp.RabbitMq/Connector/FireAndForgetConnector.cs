using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging;
using Domain.Infrastructure.Messaging.AMQP;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace Skeleton.Amqp.RabbitMq.Connector
{
    internal class FireAndForgetConnector : RabbitMqConnector, IFireAndForget
    {
        public FireAndForgetConnector(ILoggerFactory loggerFactory,
            IDependencyResolver dependencyResolver)
            : base(loggerFactory, dependencyResolver)
        {
        }

        public void SendMessage(IMessage message, MessageMetadata metadata)
        {
            try
            {
                if (!IsConnected)
                    return;

                byte[] body = SerializeMessage(message, metadata.SerializationType);

                using (IModel channel = RabbitMqConnection.CreateChannel(this))
                {
                    IBasicProperties basicProperties = RabbitMqConnection.CreateBasicProperties(channel, message, metadata);

                    string routingKey = string.IsNullOrEmpty(metadata.ReplyTo)
                        ? string.IsNullOrEmpty(metadata.Topic) ? message.GetType().Name : metadata.Topic
                        : metadata.ReplyTo;

                    Logger.LogInformation($"Send message to {metadata.Destination} of type {message.GetType().Name} with routing key: {routingKey}");

                    RabbitMqConnection.PublishMessage(channel, metadata.Destination, basicProperties, body, routingKey);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Send message exception. HostName: {BrokerConfiguration.HostName} ... ");
            }
        }

        public void SendMessage(byte[] body, IMessage message, MessageMetadata metadata)
        {
            if (!IsConnected)
                return;

            using (IModel channel = RabbitMqConnection.CreateChannel(this))
            {
                IBasicProperties basicProperties = RabbitMqConnection.CreateBasicProperties(channel, message, metadata);

                string routingKey = string.IsNullOrEmpty(metadata.ReplyTo)
                    ? string.IsNullOrEmpty(metadata.Topic) ? message.GetType().Name : metadata.Topic
                    : metadata.ReplyTo;

                Logger.LogInformation($"Send message to {metadata.Destination} of type {message.GetType().Name} with routing key: {routingKey}");

                RabbitMqConnection.PublishMessage(channel, metadata.Destination, basicProperties, body, routingKey);
            }
        }

        protected override void OnConnect(object sender, EventArgs e)
        {
            
        }
    }
}
