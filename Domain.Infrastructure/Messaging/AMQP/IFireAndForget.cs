
namespace Domain.Infrastructure.Messaging.AMQP
{
    public interface IFireAndForget: IMessageConnector
    {
        /// <summary />
        void SendMessage(IMessage message, MessageMetadata metadata);
        /// <summary />
        void SendMessage(byte[] body, IMessage message, MessageMetadata metadata);
    }
}
