using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Infrastructure.Messaging.AMQP
{
    public interface IReceiveMessageConnector
    {
        void AddReceiver(IMessageReceiver messageReceiver);
       // void AddTopic(string topic, string routerName = null, bool onlyOnExclusiveQueue = false);
    }
}
