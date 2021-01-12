using System;
using Domain.Infrastructure.Serialization;

namespace Domain.Infrastructure.Messaging
{
    public class MessageMetadata
    {
          /// <summary>
        /// 
        /// </summary>
        public const string ResponseDestinationHeaderKey = "ResponseDestination";
        /// <summary>
        /// 
        /// </summary>
        public const string RpcDestination = "RPC";
        /// <summary>
        /// 
        /// </summary>
        public const string ServerDestination = "ServerExchange";

        public const string ExchangeTypeFanout = "fanout";
        /// <summary />
        public const string ExchangeTypeDirect = "direct";
        /// <summary />
        public const string ExchangeTypeTopic = "topic";


        /// <summary>
        /// 
        /// </summary>
        public string ReplyTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? ReplyTimeoutSec { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SerializationType SerializationType { get; set; } = SerializationType.Json;
        /// <summary>
        /// 
        /// </summary>
        public ulong DeliveryTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Persistent { get; set; } = false;
    }
}
