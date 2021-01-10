using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Skeleton.Serialization.XmlSerializer
{
    public class XmlSerializer : IXmlSerializer
    {
        /// <inheritdoc />
        public SerializationType SerializerType => SerializationType.Xml;

        /// <inheritdoc />
        public byte[] ObjectToByteArray(object toSerialize)
        {
            using (var stream = new MemoryStream())
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(toSerialize.GetType());

                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    xmlSerializer.Serialize(streamWriter, toSerialize);
                }

                return stream.ToArray();
            }
        }

        /// <inheritdoc />
        public T ByteArrayToObject<T>(byte[] toDeserialize) => (T)ByteArrayToObject(toDeserialize, typeof(T), null);
        /// <inheritdoc />
        public T ByteArrayToObject<T>(byte[] toDeserialize, IReadOnlyDictionary<string, string> xmlNamespaceMapping) => (T)ByteArrayToObject(toDeserialize, typeof(T), xmlNamespaceMapping);
        /// <inheritdoc />
        public object ByteArrayToObject(byte[] toDeserialize, Type targetType) => ByteArrayToObject(toDeserialize, targetType, null);

        /// <inheritdoc />
        public object ByteArrayToObject(byte[] toDeserialize, Type targetType, IReadOnlyDictionary<string, string> xmlNamespaceMapping)
        {
            using (var stream = new MemoryStream(toDeserialize))
            {
                return StreamToObject(stream, targetType, xmlNamespaceMapping);
            }
        }

        /// <inheritdoc />
        public T StringToObject<T>(string toDeserialize) => StringToObject<T>(toDeserialize, null);

        /// <inheritdoc />
        public T StringToObject<T>(string toDeserialize, IReadOnlyDictionary<string, string> xmlNamespaceMapping)
        {
            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using (StringReader xmlTextReader = new StringReader(toDeserialize))
            {
                return xmlNamespaceMapping != null
                    ? (T)xmlSerializer.Deserialize(CreateNamespaceMapping(xmlNamespaceMapping, xmlTextReader))
                    : (T)xmlSerializer.Deserialize(xmlTextReader);
            }
        }

        /// <inheritdoc />
        public T StreamToObject<T>(Stream stream) => (T)StreamToObject(stream, typeof(T));
        /// <inheritdoc />
        public T StreamToObject<T>(Stream stream, IReadOnlyDictionary<string, string> xmlNamespaceMapping) => (T)StreamToObject(stream, typeof(T), xmlNamespaceMapping);

        /// <inheritdoc />
        public void ObjectToFile(object toSerialize, string filePath, FileMode fileMode = FileMode.Create)
        {
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                Type type = toSerialize.GetType();
                if (type.GetCustomAttribute(typeof(DataContractAttribute)) != null)
                {
                    SerializeDataContractObject(toSerialize, stream);
                }
                else
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(toSerialize.GetType());

                    using (StreamWriter streamWriter = new StreamWriter(stream))
                    {
                        xmlSerializer.Serialize(streamWriter, toSerialize);
                    }
                }
            }
        }

        private static void SerializeDataContractObject(object toSerialize, Stream writeToStream)
        {
            var serializer = new DataContractSerializer(toSerialize.GetType());
            using (var writer = new XmlTextWriter(writeToStream, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                serializer.WriteObject(writer, toSerialize);
            }
        }

        /// <inheritdoc />
        public T FileToObject<T>(string filePath) => FileToObject<T>(filePath, null);
        /// <inheritdoc />
        public T FileToObject<T>(string filePath, IReadOnlyDictionary<string, string> xmlNamespaceMapping)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                return StreamToObject<T>(fileStream, xmlNamespaceMapping);
            }
        }

        /// <inheritdoc />
        public string SerializeObject(object toSerialize)
        {
            using (var writer = new StringWriter())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(toSerialize.GetType());
                serializer.Serialize(writer, toSerialize);
                return writer.ToString();
            }
        }
        
        private object StreamToObject(Stream stream, Type targetType, IReadOnlyDictionary<string, string> xmlNamespaceMapping = null)
        {
            if (targetType.GetCustomAttribute(typeof(DataContractAttribute)) != null)
            {
                var serializer = new DataContractSerializer(targetType);
                using (var writer = xmlNamespaceMapping != null
                ? CreateNamespaceMapping(xmlNamespaceMapping, stream)
                : new XmlTextReader(stream))
                {
                    return serializer.ReadObject(writer);
                }
            }

            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(targetType);

            using (StreamReader sr = new StreamReader(stream))
            {
                return xmlNamespaceMapping != null
                    ? xmlSerializer.Deserialize(CreateNamespaceMapping(xmlNamespaceMapping, sr))
                    : xmlSerializer.Deserialize(sr);
            }
        }

        private XmlParserContext CreateParserContext(IReadOnlyDictionary<string, string> xmlMapping)
        {
            // Create a new NameTable
            NameTable nt = new NameTable();

            // Create a new NamespaceManager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);

            foreach (var pair in xmlMapping)
            {
                nsmgr.AddNamespace(pair.Key, pair.Value);
            }

            return new XmlParserContext(null, nsmgr, null, XmlSpace.None);
        }

        private XmlReaderSettings ReaderSettings => new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        private XmlReader CreateNamespaceMapping(IReadOnlyDictionary<string, string> xmlMapping, Stream sr) => XmlReader.Create(sr, ReaderSettings, CreateParserContext(xmlMapping));
        
        private XmlReader CreateNamespaceMapping(IReadOnlyDictionary<string, string> xmlMapping, TextReader sr) => XmlReader.Create(sr, ReaderSettings, CreateParserContext(xmlMapping));
        
    }
}
