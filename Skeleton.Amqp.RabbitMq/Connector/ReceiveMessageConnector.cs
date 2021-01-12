using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging;
using Domain.Infrastructure.Messaging.AMQP;
using Domain.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Skeleton.Amqp.RabbitMq.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq.Connector
{
    internal class ReceiveMessageConnector : RabbitMqConnector, IReceiveMessageConnector
    {
        private readonly Dictionary<Type, List<Action<object, MessageMetadata>>> _registerMethodDictionary =
            new Dictionary<Type, List<Action<object, MessageMetadata>>>();

        private EventingBasicConsumer _eventingBasicConsumer;

        public ReceiveMessageConnector(ILoggerFactory loggerFactory,
            IDependencyResolver dependencyResolver)
            : base(loggerFactory, dependencyResolver)
        {
        }

        protected override void OnConnect(object sender, EventArgs e)
        {
            if (RabbitMqConnection.RegisteredConnectors.ContainsKey(Id))
                return;
            IModel channel = RabbitMqConnection.CreateChannel(this);
            RabbitMqConnection.ConfigureChannel(channel, BrokerConfiguration.ChannelConfiguration);
            _eventingBasicConsumer = RabbitMqConnection.CreateEventingBasicConsumer(channel);
            _eventingBasicConsumer.Received += EventingBasicConsumerOnReceived;

            foreach (BrokerSubscriberConfiguration config in BrokerConfiguration.BrokerSubscriberConfiguration)
            {
                RabbitMqConnection.CreateExchange(channel, config.ExchangeName, config.ExchangeType);

                config.QueueName = RabbitMqConnection.CreateQueue(channel,
                    config.QueueName,
                    config.Exclusive,
                    config.Durable,
                    config.AutoDelete);

                switch (config.ExchangeType)
                {
                    case ExchangeType.Topic:
                    case ExchangeType.Direct:
                        if (config.Topic != null)
                        {
                            foreach (string topic in config.Topic)
                            {
                                RabbitMqConnection.CreateBinding(channel, config.QueueName, config.ExchangeName, topic);
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<Type, List<Action<object, MessageMetadata>>> obj in _registerMethodDictionary)
                            {
                                if (!obj.Key.IsGenericType)
                                    RabbitMqConnection.CreateBinding(channel, config.QueueName, config.ExchangeName, obj.Key.Name);
                            }
                        }
                        break;
                    case ExchangeType.Fanout:
                        RabbitMqConnection.CreateBinding(channel, config.QueueName, config.ExchangeName);
                        break;
                }

                RabbitMqConnection.ConsumeQueue(this,
                    channel,
                    config.QueueName,
                    Id.ToString(),
                    true,
                    _eventingBasicConsumer);

            }
        }

        private void EventingBasicConsumerOnReceived(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                string typeString = args.BasicProperties.Type;

                Type type = null;
                if (!string.IsNullOrWhiteSpace(typeString))
                    type = Type.GetType(typeString, false);

                if (type == null)
                {
                    throw new TypeLoadException($"Unknown type {typeString}. Failed to properly consume message.");
                }

                IMessage message;

                bool success = Enum.TryParse(args.BasicProperties.ContentEncoding,
                    true,
                    out SerializationType serializationType);
                if (success)
                {
                    message = DeserializeMessage(args.Body, type, serializationType);
                }
                else
                {
                    Logger.LogError(
                        $"Cannot deserialize object type {type.Name} serialize in {args.BasicProperties.ContentEncoding}");
                    return;
                }

                foreach (var call in _registerMethodDictionary[type])
                {
                    Task.Factory.StartNew(() => call(message,
                        RabbitMqConnection.CreateMetadata(args.BasicProperties,
                            args.RoutingKey,
                            Serializers[SerializationType.Binary],
                            args.DeliveryTag)));
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Message reception error for {args.BasicProperties.Type}");
            }

        }

        public void AddReceiver(IMessageReceiver messageReceiver)
        {
            RegisterMessageReceiver(messageReceiver);
            //IModel channel = RabbitMqConnection.GetAssociatedChannel(this);
            //if (channel == null)
            //    return;
            //foreach (BrokerSubscriberConfiguration config in _realConfiguration)
            //{
            //    if ((config.RouterType == ExchangeType.Topic || config.RouterType == ExchangeType.Direct) && config.Topic == null)
            //    {
            //        foreach (KeyValuePair<Type, List<Action<object, MessageMetadata>>> obj in _registerMethodDictionary)
            //        {
            //            if (!obj.Key.IsGenericType)
            //                RabbitMqConnection.CreateBinding(channel, config.QueueName, config.RouterName, obj.Key.Name);
            //        }
            //    }
            //}
        }

        public void AddTopic(string topic, string routerName = null, bool onlyOnExclusiveQueue = false)
        {
            throw new NotImplementedException();
        }

        public uint QueuePurge(string queueName)
        {
            throw new NotImplementedException();
        }

        private void RegisterMessageReceiver(IMessageReceiver messageReceiver)
        {
            Type type = messageReceiver.GetType();

            IEnumerable<Type> interfaces =
                type.GetInterfaces()
                    .Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IMessageReceiver<>));

            foreach (Type it in interfaces)
            {
                foreach (MethodInfo methodInfo in it.GetMethods())
                {
                    Type parameterType = methodInfo.GetParameters()[0].ParameterType;

                    ConstantExpression instance = Expression.Constant(messageReceiver);

                    var messageParameter = Expression.Parameter(typeof(object));
                    var messageConverter = Expression.Convert(messageParameter, parameterType);

                    var metadataParameter = Expression.Parameter(typeof(object));
                    var metadataConverter = Expression.Convert(metadataParameter, typeof(MessageMetadata));

                    var methodCall = Expression.Call(instance, methodInfo, messageConverter, metadataConverter);
                    if (!_registerMethodDictionary.ContainsKey(parameterType))
                    {
                        _registerMethodDictionary[parameterType] = new List<Action<object, MessageMetadata>>();
                    }
                    _registerMethodDictionary[parameterType]
                        .Add(Expression.Lambda<Action<object, MessageMetadata>>(methodCall, messageParameter, metadataParameter).Compile());
                }
            }
        }

    }
}
