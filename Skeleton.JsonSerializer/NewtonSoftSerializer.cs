using Domain.Infrastructure.Serialization;
using Domain.Infrastructure.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Skeleton.Serialization.NewtonSoft.Contract;
using Skeleton.Serialization.NewtonSoft.Converter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Skeleton.Serialization.NewtonSoft
{
    public class NewtonSoftSerializer : IJsonSerializer
    {
        private readonly JsonSerializer _jsonSerializer = JsonSerializer.Create(
              new JsonSerializerSettings
              {
                  NullValueHandling = NullValueHandling.Ignore,
                  DefaultValueHandling = DefaultValueHandling.Include,
                  TypeNameHandling = TypeNameHandling.Auto,
                  ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                  MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                  ConstructorHandling = ConstructorHandling.Default,
                  PreserveReferencesHandling = PreserveReferencesHandling.None,
                  CheckAdditionalContent = false,
                  StringEscapeHandling = StringEscapeHandling.Default,
                  Formatting = Formatting.Indented,
                  ObjectCreationHandling = ObjectCreationHandling.Replace,
                  MissingMemberHandling = MissingMemberHandling.Ignore,
                  Converters = new List<JsonConverter>(),
              });

         public SerializationType SerializerType => SerializationType.Json;

        /// <inheritdoc />
        public byte[] ObjectToByteArray(object toSerialize)
        {
            using (var stream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    _jsonSerializer.Serialize(streamWriter, toSerialize, toSerialize.GetType());
                }
                return stream.ToArray();
            }
        }

        /// <inheritdoc />
        public T ByteArrayToObject<T>(byte[] toDeserialize)
        {
            return (T)ByteArrayToObject(toDeserialize, typeof(T));
        }

        /// <inheritdoc />
        public object ByteArrayToObject(byte[] toDeserialize, Type targetType)
        {
            using (MemoryStream ms = new MemoryStream(toDeserialize))
            {
                return StreamToObject(ms, targetType);
            }
        }

        /// <inheritdoc />
        public T StringToObject<T>(string toDeserialize)
        {
            using (JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(toDeserialize)))
                return _jsonSerializer.Deserialize<T>(jsonTextReader);
        }

        /// <inheritdoc />
        public T StreamToObject<T>(Stream stream)
        {
            return (T)StreamToObject(stream, typeof(T));
        }

        /// <inheritdoc />
        public void ObjectToFile(object toSerialize, string filePath, FileMode fileMode = FileMode.Create)
        {
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    _jsonSerializer.Serialize(streamWriter, toSerialize, toSerialize.GetType());
                }
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
        public string SerializeObject(object toSerialize)
        {
            StringWriter stringWriter = new StringWriter(new StringBuilder(), CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.Formatting = _jsonSerializer.Formatting;
                _jsonSerializer.Serialize(jsonTextWriter, toSerialize, toSerialize.GetType());
            }
            return stringWriter.ToString();
        }

        /// <inheritdoc />
        public dynamic StringToDynamic(string toDeserialize) => JObject.Parse(toDeserialize);

        /// <inheritdoc />
        public void SetCustomContractResolver<T>()
            where T : class, IJsonCustomContract, new()
        {
            _jsonSerializer.ContractResolver = new CustomContractWrapper<T>();
        }

        /// <inheritdoc />
        public void AddCustomJsonConverter<T>()
            where T : class, IJsonCustomConverter, new()
        {
            _jsonSerializer.Converters.Add(new CustomConverterWrapper<T>());
        }

        /// <inheritdoc />
        public void UseCamelCaseContractResolver()
        {
            _jsonSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private object StreamToObject(Stream stream, Type targetType)
        {
            using (var reader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _jsonSerializer.Deserialize(jsonReader, targetType);
                }
            }
        }
    }
}
