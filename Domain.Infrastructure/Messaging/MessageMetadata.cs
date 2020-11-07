using System;
using Domain.Infrastructure.Serialization;

namespace Domain.Infrastructure.Messaging
{
    public class MessageMetadata
    {
        public string ReplyTo { get; set; }
        public double? ReplyTimeoutSec { get; set; }
        public Guid MessageId { get; set; }
        public string Destination { get; set; }
        public string Topic { get; set; }
        /// <summary />
        public SerializationType SerializationType { get; set; } = SerializationType.Binary;
    }
}
