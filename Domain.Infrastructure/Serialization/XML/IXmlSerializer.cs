using System;
using System.Collections.Generic;
using System.IO;

namespace Domain.Infrastructure.Serialization.XML
{
     /// <summary />
    public interface IXmlSerializer : ISerializer
    {
        /// <summary />
        T FileToObject<T>(string filePath, IReadOnlyDictionary<string, string> xmlNamespaceMapping);

        /// <summary />
        T StreamToObject<T>(Stream stream, IReadOnlyDictionary<string, string> xmlNamespaceMapping);

        /// <summary />
        T StringToObject<T>(string toDeserialize, IReadOnlyDictionary<string, string> xmlNamespaceMapping);

        /// <summary />
        T ByteArrayToObject<T>(byte[] toDeserialize, IReadOnlyDictionary<string, string> xmlNamespaceMapping);

        /// <summary />
        object ByteArrayToObject(byte[] toDeserialize, Type targetType, IReadOnlyDictionary<string, string> xmlNamespaceMapping);
    }
}
