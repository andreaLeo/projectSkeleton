using Domain.Infrastructure.Serialization.Json;
using Newtonsoft.Json;
using System;

namespace Skeleton.Serialization.NewtonSoft.Converter
{
    internal class CustomConverterWrapper<T> : JsonConverter
        where T : class, IJsonCustomConverter, new()
    {
        private readonly T _customConverter = new T();

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!_customConverter.CanWrite)
                throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");

            if (_customConverter.WriteJson(value, out object result))
            {
                if (Type.GetTypeCode(result.GetType()) == TypeCode.Object)
                {
                    serializer.Serialize(writer, result);
                }
                else
                {
                    writer.WriteValue(result);
                }
            }
            else
            {
                writer.WriteValue(value);
            }

        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!_customConverter.CanRead)
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");

            return _customConverter.ReadJson(objectType, reader.Value, out object result) ? result : null;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => _customConverter.CanConvert(objectType);

        /// <inheritdoc />
        public override bool CanRead => _customConverter.CanRead;

        /// <inheritdoc />
        public override bool CanWrite => _customConverter.CanWrite;
    }
}
