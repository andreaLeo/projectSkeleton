using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Infrastructure.Messaging.AMQP
{
    public interface IRpcMessageConnector : IMessageConnector
    {
        /// <summary />
        Tuple<TResponse, MessageMetadata, string> Request<TResponse>(IMessage message, MessageMetadata metadata)
          where TResponse : class, IMessage;

        /// <summary />
        Task<Tuple<TResponse, MessageMetadata, string>> RequestAsync<TResponse>(IMessage message, MessageMetadata metadata)
          where TResponse : class, IMessage;
    }
}
