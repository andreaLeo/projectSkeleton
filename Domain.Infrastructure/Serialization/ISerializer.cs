using System;
using System.IO;

namespace Domain.Infrastructure.Serialization
{
     public interface ISerializer
    {
        /// <summary />
        SerializationType SerializerType { get; }

        /// <summary />
        byte[] ObjectToByteArray(object toSerialize);

        /// <summary />
        T ByteArrayToObject<T>(byte[] toDeserialize);

        /// <summary />
        object ByteArrayToObject(byte[] toDeserialize, Type targetType);

        /// <summary />
        T StringToObject<T>(string toDeserialize);

        /// <summary />
        T StreamToObject<T>(Stream stream);

        /// <summary />
        void ObjectToFile(object toSerialize, string filePath, FileMode fileMode = FileMode.Create);

        /// <summary />
        T FileToObject<T>(string filePath);

        /// <summary />
        string SerializeObject(object toSerialize);
    }
}
