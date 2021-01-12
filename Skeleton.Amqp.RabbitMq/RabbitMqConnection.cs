using Domain.Infrastructure.Messaging;
using Domain.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Skeleton.Amqp.RabbitMq.Configuration;
using Skeleton.Amqp.RabbitMq.Connector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Skeleton.Amqp.RabbitMq
{
    public sealed class RabbitMqConnection
    {
        private readonly ILogger _logger;

        private IConnection _connection;
        private const string DEAD_LETTER = "DeadLetter";
        private readonly Timer _connectionRetryTimer;
        private ConnectionFactory _factory;
        private BrokerConfiguration _configuration;
        public event EventHandler OnConnect;


        public RabbitMqConnection(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RabbitMqConnection>();
            _connectionRetryTimer = new Timer((state) =>
            {
                try
                {
                    CreateConnection(_configuration);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _factory = null;
                    _connection = null;
                }
            }, this, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }

        internal void ConfigureChannel(IModel channel, BrokerSubscriberChannel channelConfig) => channel.BasicQos(0, channelConfig.ParallelMessage, false);

        /// <summary>
        /// Specific arg to link created queue ton dead letter exchange in order to track error is message receiver throw an exception
        /// </summary>
        private readonly Dictionary<string, object> _deadLetterArguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = DEAD_LETTER,
        };

        /// <summary>
        /// Represent all manager connection close reason.
        /// </summary>
        public readonly Dictionary<ushort, string> CloseCodeReasonDictionary = new Dictionary<ushort, string>
        {
            [0] = $"{nameof(RabbitMqConnection)} is disposed.",
            [1] = "Client disconnected.",
            [2] = $"{nameof(RpcMessageConnector)} timeout.",
        };

        public void Connect(BrokerConfiguration configuration)
        {
            CreateConnection(configuration);
        }

        private void CreateConnection(BrokerConfiguration configuration)
        {
            try
            {
                if (!IsConnect)
                {
                    if (_factory == null)
                    {
                        _factory = new ConnectionFactory
                        {
                            HostName = configuration.HostName,
                            Port = configuration.Port,
                            UserName = configuration.UserName,
                            Password = configuration.Password,
                            VirtualHost = configuration.Vhost,
                        };

#if DEBUG
                        // Force to use Ipv4 instead of trying ipv6 and try catch exception in his code.
                        _factory.SocketFactory = family => ConnectionFactory.DefaultSocketFactory(AddressFamily.InterNetwork);
#endif
                    }

                    RegisteredConnectors.Clear();

                    _connection = _factory.CreateConnection($"{Environment.MachineName}:{Environment.UserName}");
                    _logger.LogInformation($"Connected on broker {_factory.HostName} - vHost: {_factory.VirtualHost}");

                    using (IModel channel = _connection.CreateModel())
                    {
                        CreateDeadLetter(channel);
                    }

                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _configuration = configuration;
                }
                if (IsConnect)
                    OnConnect?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Connection failed.");
            }
        }



        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            try
            {
                // Check if shutdown code is known in our managed shut down code .
                // If yes check if they no more connected connector to really shut down the connection 
                // Reminder all connectors in the same process share the connection on Rabbit mq
                // Connection object is thread safe.
                if (CloseCodeReasonDictionary.ContainsKey(shutdownEventArgs.ReplyCode)
                     && RegisteredConnectors.Count == 0)
                    return;

                _logger.LogWarning($"Disconnect for bad reason id: {shutdownEventArgs.ReplyCode} - Text: {shutdownEventArgs.ReplyText} - Initiator: {shutdownEventArgs.Initiator}");
                _connection = null;
                _factory = null;
                RegisteredConnectors.Clear();

                // If the shutdown reason is unknown (ei: send message connector try
                // to send a message on a missing exchange) we recreate the shared connection
                CreateConnection(_configuration);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        internal bool IsConnect => _connection != null && _connection.IsOpen;

        internal IModel GetAssociatedChannel(IMessageConnector connector)
        {
            IModel channel = null;
            if (IsConnect && RegisteredConnectors.TryGetValue(connector.Id, out channel))
            {
                if (channel == null || channel.IsOpen)
                    RegisteredConnectors.TryRemove(connector.Id, out _);
            }

            return channel;
        }

        public IModel CreateChannel(IMessageConnector connector)
        {
            IModel channel = _connection.CreateModel();

            channel.ModelShutdown += (sender, args) =>
            {
                _logger.LogInformation($"Channel shutdown for connector Id: {connector.Id} - type {connector.GetType().Name} - reason id: {args.ReplyCode} - Text: {args.ReplyText} - Initiator: {args.Initiator}");
                RegisteredConnectors.TryRemove(connector.Id, out _);
            };

            RegisterConnector(connector, channel);

            return channel;
        }

        internal ConcurrentDictionary<Guid, IModel> RegisteredConnectors { get; } = new ConcurrentDictionary<Guid, IModel>();

        internal void RegisterConnector(IMessageConnector connector, IModel channel)
        {
            RegisteredConnectors[connector.Id] = channel;
            _logger.LogInformation($"Register channel for connector Id: {connector.Id} - type {connector.GetType().Name}.");
        }

        internal void PublishMessage(IModel channel, string destination, IBasicProperties basicProperties, byte[] body, string routingKey = "") => channel.BasicPublish(destination, routingKey, basicProperties, body);

        internal EventingBasicConsumer CreateEventingBasicConsumer(IModel channel) => new EventingBasicConsumer(channel);

        internal IBasicProperties CreateBasicProperties(IModel channel, IMessage message, MessageMetadata metadata = null)
        {
            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Type = $"{message.GetType().FullName}, {message.GetType().Assembly.GetName().Name}";

            if (properties.Type.Length > 255)
                throw new ArgumentOutOfRangeException($"Message Type is too long {properties.Type} max length is 255 (current: {properties.Type.Length})");

            properties.CorrelationId = string.IsNullOrEmpty(metadata?.MessageId)
                ? Guid.NewGuid().ToString()
                : metadata.MessageId;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.DeliveryMode = 1;

            if (metadata != null)
                properties.Persistent = metadata.Persistent;

            properties.ContentEncoding = Enum.GetName(typeof(SerializationType), metadata?.SerializationType ?? SerializationType.Binary);

            return properties;
        }

        internal void CreateDeadLetter(IModel channel)
        {
            channel.ExchangeDeclare(DEAD_LETTER, ExchangeType.Direct, true);
            channel.QueueDeclare(DEAD_LETTER, true, false, false, null);
            channel.QueueBind(DEAD_LETTER, DEAD_LETTER, "*");
        }

        internal string CreateQueue(IModel channel, string queueName, bool exclusiveOnConnection, bool durable, bool autoDelete)
        {
            string realQueueName = queueName;
            if (string.IsNullOrEmpty(realQueueName))
            {
                realQueueName = Guid.NewGuid().ToString();
            }
            else if (exclusiveOnConnection)
            {
                realQueueName += $"_{Guid.NewGuid()}";
            }

            return channel.QueueDeclare(realQueueName, durable, exclusiveOnConnection, autoDelete, _deadLetterArguments);
        }

        internal void CreateExchange(IModel channel, string exchangeName, string exchangeType) => channel.ExchangeDeclare(exchangeName, exchangeType, true);

        internal void CreateBinding(IModel channel, string queueName, string exchangeName, string routingKey = "*")
        {
            _logger.LogInformation($"{nameof(CreateBinding)} for {nameof(queueName)}:{queueName} - {nameof(exchangeName)}:{exchangeName} - {nameof(routingKey)}:{routingKey}");
            channel.QueueBind(queueName, exchangeName, routingKey);
        }

        internal void Disconnect(IMessageConnector connector, ushort closeCode)
        {
            if (!RegisteredConnectors.ContainsKey(connector.Id))
                return;
            Close(connector, closeCode);
        }

        internal void ConsumeQueue(IMessageConnector connector, IModel channel, string queueName, string consumerTag, bool autoAck, IBasicConsumer consumer) =>  channel.BasicConsume(queueName, autoAck, Guid.NewGuid().ToString(), consumer);

        internal uint QueueConsumerCount(IMessageConnector connector, string queueName)
        {
            try
            {
                uint ret = 0;
                QueueDeclareOk ok = null;
                IModel channel = GetAssociatedChannel(connector);
                if (channel != null)
                {
                    ok = channel.QueueDeclarePassive(queueName);
                }
                else if (IsConnect)
                {
                    using (channel = CreateChannel(connector))
                    {
                        ok = channel.QueueDeclarePassive(queueName);
                    }
                }
                if (ok != null)
                    ret = ok.ConsumerCount;
                return ret;
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
            {
                _logger.LogWarning(ex.Message);
                return 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        private void Close(IMessageConnector connector, ushort closeCode)
        {
            IModel channel = GetAssociatedChannel(connector);
            channel?.Close(closeCode, CloseCodeReasonDictionary[closeCode]);
            if (RegisteredConnectors.Count == 0 && IsConnect)
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.Close(closeCode, CloseCodeReasonDictionary[closeCode]);
                _factory = null;
            }
        }


        internal MessageMetadata CreateMetadata(IBasicProperties properties, string topic, ISerializer serializer, ulong? deliveryTag = null)
        {
            MessageMetadata metadata = new MessageMetadata
            {
                ReplyTo = properties.ReplyTo,
                MessageId = properties.CorrelationId,
                Destination =
                    properties.IsHeadersPresent() &&
                    properties.Headers.ContainsKey(MessageMetadata.ResponseDestinationHeaderKey)
                        ? serializer.ByteArrayToObject<string>(properties.Headers[MessageMetadata.ResponseDestinationHeaderKey] as byte[])
                        : null,
                Topic = topic
            };
            if (deliveryTag != null)
                metadata.DeliveryTag = deliveryTag.Value;
            return metadata;
        }

        /// <summary>Purge a queue of messages</summary>
        /// <returns>Returns the number of messages purged.</returns>
        internal uint QueuePurge(IModel channel, string queueName) => channel.QueuePurge(queueName);
    }
}
