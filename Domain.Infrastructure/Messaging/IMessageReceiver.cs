using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Infrastructure.Messaging
{
    public interface IMessageReceiver { }

    public interface IMessageReceiver<TDTO> : IMessageReceiver
        where TDTO : IMessage
    {
        void Receive(TDTO message, MessageMetadata metadata);
    }
}
