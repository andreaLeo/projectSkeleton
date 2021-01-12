namespace Skeleton.Amqp.RabbitMq.Configuration
{
    public class BrokerSubscriberConfiguration
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }

        /// <summary>
        /// Durable (the queue will survive a broker restart)
        /// </summary>
        public bool Durable { get; set; }

        /// <summary>
        /// Exclusive (used by only one connection and the queue will be deleted when that connection closes)
        /// </summary>
        public bool Exclusive { get; set; } = true;

        /// <summary>
        /// Auto-delete (queue that has had at least one consumer is deleted when last consumer unsubscribes)
        /// </summary>
        public bool AutoDelete { get; set; } = true;

        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Topic;

        public string[] Topic { get; set; }
    }
}
