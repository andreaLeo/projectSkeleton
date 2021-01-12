using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Messaging;
using Domain.Infrastructure.Messaging.AMQP;
using Domain.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Skeleton.Amqp.RabbitMq.Connector.Rpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq.Connector
{
    internal class RpcMessageConnector : RabbitMqConnector, IRpcMessageConnector
    {
        private const double DefaultReplyTimeout = 60;

        private readonly ConcurrentDictionary<string, ResponseAction> _responseActions =
            new ConcurrentDictionary<string, ResponseAction>();

        public RpcMessageConnector(ILoggerFactory loggerFactory,
            IDependencyResolver dependencyResolver)
            : base(loggerFactory, dependencyResolver)
        {
        }

        public Tuple<TResponse, MessageMetadata, string> Request<TResponse>(IMessage message, MessageMetadata metadata) 
            where TResponse : class, IMessage
        {
            var rpcDataRequestAsync = RequestAsync<TResponse>(message, metadata);
           
            bool timeout = rpcDataRequestAsync.Wait(metadata.ReplyTimeoutSec != null
                ? TimeSpan.FromSeconds(metadata.ReplyTimeoutSec.Value)
                : TimeSpan.FromSeconds(DefaultReplyTimeout));

            if (!timeout)
                ForgetMessage(rpcDataRequestAsync.Result.Item3);

            return timeout ? rpcDataRequestAsync.Result : null;
        }

        public Task<Tuple<TResponse, MessageMetadata, string>> RequestAsync<TResponse>(IMessage message, MessageMetadata metadata)
            where TResponse : class, IMessage
        {
            string queueName = null;
            try
            {
                var taskCompletionSource = new TaskCompletionSource<Tuple<TResponse, MessageMetadata, string>>();
                string routingKey = string.IsNullOrEmpty(metadata.ReplyTo)
                    ? string.IsNullOrEmpty(metadata.Topic) ? message.GetType().Name : metadata.Topic
                    : metadata.ReplyTo;

                IModel channel = RabbitMqConnection.CreateChannel(this);
                queueName = RabbitMqConnection.CreateQueue(channel, string.Empty, true, false, false);

                metadata.MessageId = string.IsNullOrEmpty(metadata.ReplyTo) ? queueName : metadata.ReplyTo;
                metadata.ReplyTo = queueName;

                RabbitMqConnection.CreateBinding(channel, queueName, MessageMetadata.RpcDestination, metadata.ReplyTo);
                EventingBasicConsumer consumer = RabbitMqConnection.CreateEventingBasicConsumer(channel);

                consumer.Received += ConsumerOnReceived;
                RabbitMqConnection.ConsumeQueue(this, channel, queueName, string.Empty, true, consumer);

                IBasicProperties basicProperties = RabbitMqConnection.CreateBasicProperties(channel, message, metadata);
                basicProperties.ReplyTo = string.IsNullOrEmpty(metadata.ReplyTo) ? queueName : metadata.ReplyTo;

                if (!basicProperties.IsHeadersPresent())
                    basicProperties.Headers = new Dictionary<string, object>();
                basicProperties.Headers[MessageMetadata.ResponseDestinationHeaderKey] = MessageMetadata.RpcDestination;
                RegisterResponseAction(basicProperties.CorrelationId, queueName, consumer, typeof(TResponse), taskCompletionSource);

                byte[] body = SerializeMessage(message, metadata.SerializationType);

                Logger.LogInformation(
                    $"Send request to: {metadata.Destination} with routingkey: {routingKey} - ReplyTo: {basicProperties.ReplyTo} of type: {typeof(TResponse).Name} id: {basicProperties.CorrelationId}");
                RabbitMqConnection.PublishMessage(channel, metadata.Destination, basicProperties, body, routingKey);
                return taskCompletionSource.Task;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(RequestAsync)}");
                if (queueName != null)
                    ForgetMessage(queueName);
                throw;
            }
        }

        private void ForgetMessage(string correlationId)
        {
            if (_responseActions.TryRemove(correlationId, out ResponseAction responseAction))
            {
                Logger.LogWarning($"Message {correlationId} forget from {nameof(RpcMessageConnector)}");

                EventingBasicConsumer consumer = responseAction.Consumer as EventingBasicConsumer;
                if (consumer == null) return;
                consumer.Received -= ConsumerOnReceived;
                consumer.Model.QueueDelete(responseAction.ResponseQueue);
                consumer.Model.Close(2, RabbitMqConnection.CloseCodeReasonDictionary[2]);
            }
        }

        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            if (sender is EventingBasicConsumer consumer)
            {
                consumer.Received -= ConsumerOnReceived;
            }

            if (_responseActions.TryRemove(basicDeliverEventArgs.BasicProperties.CorrelationId, out ResponseAction responseAction))
            {
                Logger.LogInformation($"Response received for message {basicDeliverEventArgs.BasicProperties.CorrelationId}");
                IMessage message;

                bool success = Enum.TryParse(basicDeliverEventArgs.BasicProperties.ContentEncoding, 
                    true, 
                    out SerializationType serializationType);

                if (success)
                {
                    message = DeserializeMessage(basicDeliverEventArgs.Body, responseAction.ExpectedResponse, serializationType);
                }
                else
                {
                    Logger.LogError($"Cannot deserialize object type { responseAction.ExpectedResponse} serialize in {basicDeliverEventArgs.BasicProperties.ContentEncoding}");
                    return;
                }
                responseAction.OnSuccess(message,
                    RabbitMqConnection.CreateMetadata(basicDeliverEventArgs.BasicProperties, 
                    basicDeliverEventArgs.RoutingKey,
                    Serializers[SerializationType.Binary], 
                    basicDeliverEventArgs.DeliveryTag));
            }
        }

        private void RegisterResponseAction<TResponse>(string correlationId, string responseQueue, IBasicConsumer consumer, Type expectedResponse, TaskCompletionSource<Tuple<TResponse, MessageMetadata, string>> taskCompletionSource)
            where TResponse : class, IMessage
        {
            _responseActions.TryAdd(correlationId, new ResponseAction
            {
                OnSuccess = (message, metaData) =>
                {
                    TResponse response = (TResponse)message;

                    consumer.Model.QueueDelete(metaData.Topic);
                    consumer.Model.Close(1, RabbitMqConnection.CloseCodeReasonDictionary[1]);

                    taskCompletionSource.TrySetResult(new Tuple<TResponse, MessageMetadata, string>(response, metaData, correlationId));
                },
                OnFailure = () =>
                {
                    consumer.Model.QueueDelete(responseQueue);
                    consumer.Model.Close(1, RabbitMqConnection.CloseCodeReasonDictionary[1]);

                    taskCompletionSource.SetException(new NotImplementedException());
                },
                Consumer = consumer,
                ExpectedResponse = expectedResponse,
                ResponseQueue = responseQueue,
            });
        }

        protected override void OnConnect(object sender, EventArgs e)
        {
            using (IModel channel = RabbitMqConnection.CreateChannel(this))
            {
                RabbitMqConnection.CreateExchange(channel, MessageMetadata.RpcDestination, ExchangeType.Topic);
            }
        }
    }
}
