using System;

namespace Domain.Infrastructure.Messaging
{
    public interface IMessageConnector : IDisposable
    {
         /// <summary />
        void Connect();
      
        /// <summary />
        void Disconnect();
        /// <summary />
        bool IsConnected { get; }
        /// <summary />
        Guid Id { get; }
    }
}
