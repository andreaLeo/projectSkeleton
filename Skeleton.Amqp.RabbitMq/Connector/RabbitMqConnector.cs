using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging;
using Domain.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using Skeleton.Amqp.RabbitMq.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Skeleton.Amqp.RabbitMq.Connector
{
    internal abstract class RabbitMqConnector : IMessageConnector
    {
        private const string RabbitMqConfig = "rabbitmq.json";

        protected RabbitMqConnection RabbitMqConnection { get; }
        protected BrokerConfiguration BrokerConfiguration { get; private set; }
        public bool IsConnected => RabbitMqConnection.IsConnect;
        public Guid Id { get; } = Guid.NewGuid();
        protected Dictionary<SerializationType, ISerializer> Serializers { get; } = new Dictionary<SerializationType, ISerializer>();

        protected ILogger Logger { get; private set;}

        protected RabbitMqConnector(
            ILoggerFactory loggerFactory,
            IDependencyResolver dependencyResolver)
        {
            Logger = loggerFactory.CreateLogger(GetType());
            RabbitMqConnection = dependencyResolver.Resolve<RabbitMqConnection>();

            foreach (var serializer in dependencyResolver.ResolveAll<ISerializer>())
            {
                Serializers[serializer.SerializerType] = serializer;
            }
        }

        public void Dispose() { }

        public virtual void Connect()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), RabbitMqConfig);

            if (File.Exists(filePath))
            {
                BrokerConfiguration = Serializers[SerializationType.Json].FileToObject<BrokerConfiguration>(filePath);
            }

            RabbitMqConnection.OnConnect += OnConnect;
            RabbitMqConnection.Connect(BrokerConfiguration);
        }

        protected abstract void OnConnect(object sender, EventArgs e);

        public virtual void Disconnect() { }

        public uint IsQueueAvailable(string queueName)
        {
            throw new NotImplementedException();
        }

        protected IMessage DeserializeMessage(ReadOnlyMemory<byte> body, Type objectType, SerializationType serializationType)
        {
            IMessage message = null;

            if (Serializers.TryGetValue(serializationType, out ISerializer serializer))
            {
                message = (IMessage)serializer.ByteArrayToObject(body.ToArray(), objectType);
            }
            else
            {
                Logger.LogError($"Cannot find serializer {serializationType} for Type {objectType.FullName} ...");
            }

            return message;
        }

        protected byte[] SerializeMessage(IMessage message, SerializationType serializationType)
        {
            byte[] body = null;

            if (Serializers.TryGetValue(serializationType, out ISerializer serializer))
            {
                body = serializer.ObjectToByteArray(message);
            }

            return body;
        }
    }
}
