using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.BinarySerializer;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Skeleton.Serialization.Binary
{
    public class BinarySerializer : IBinarySerializer
    {
        /// <inheritdoc />
        public SerializationType SerializerType => SerializationType.Binary;

        /// <inheritdoc />
        public byte[] ObjectToByteArray(object toSerialize)
        {
            if (toSerialize is MemoryStream stream)
                return stream.ToArray();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, toSerialize);
                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }

        /// <inheritdoc />
        public T ByteArrayToObject<T>(byte[] toDeserialize) => (T) ByteArrayToObject(toDeserialize, typeof(T));

        /// <inheritdoc />
        public object ByteArrayToObject(byte[] toDeserialize, Type targetType)
        {
            if (targetType == typeof(string))
                return Convert.ChangeType(System.Text.Encoding.Default.GetString(toDeserialize), targetType);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(toDeserialize, 0, toDeserialize.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return StreamToObject<object>(memoryStream);
            }
        }

        /// <inheritdoc />
        public T StringToObject<T>(string toDeserialize)
        {
            byte[] b = Convert.FromBase64String(toDeserialize);
            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <inheritdoc />
        public T StreamToObject<T>(Stream stream)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return (T)binaryFormatter.Deserialize(stream);
        }

        /// <inheritdoc />
        public void ObjectToFile(object toSerialize, string filePath, FileMode fileMode = FileMode.Create)
        {
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, toSerialize);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        /// <inheritdoc />
        public T FileToObject<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                return StreamToObject<T>(stream);
            }
        }

        /// <inheritdoc />
        public string SerializeObject(object toSerialize) => ObjectToByteArray(toSerialize).ToString();
    }
}
