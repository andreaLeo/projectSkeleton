namespace Skeleton.Amqp.RabbitMq.Configuration
{
    public class BrokerSubscriberChannel
    {
        /// <summary>
        /// 0 equals to not limit parallel message
        /// </summary>
        public ushort ParallelMessage { get; set; } = 0;
    }
}
