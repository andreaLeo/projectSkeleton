using System.Collections.Generic;

namespace Skeleton.Amqp.RabbitMq.Configuration
{
    public class BrokerConfiguration
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Vhost { get; set; }

        public BrokerSubscriberChannel ChannelConfiguration { get; set; } = new BrokerSubscriberChannel();
        public List<BrokerSubscriberConfiguration> BrokerSubscriberConfiguration { get; set; } = new List<BrokerSubscriberConfiguration>();
    }
}
