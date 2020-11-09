using System;

namespace Domain.Infrastructure.Serialization.Json
{
    public interface IJsonCustomConverter
    {
        /// <summary>
        /// Checks that this converter can manage the given <paramref name="objectType"/>.
        /// </summary>
        bool CanConvert(Type objectType);

        /// <summary>
        /// Indicates if this converter handles reading.
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Reads a value.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="result">Read value, null otherwise.</param>
        /// <returns>True if value was read and converted, false otherwise.</returns>
        bool ReadJson(Type objectType, object existingValue, out object result);

        /// <summary>
        /// Indicates if this converter handles writing.
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <param name="value">Value to convert for write.</param>
        /// <param name="result">Value to write.</param>
        /// <returns>True if value was converted for write, false otherwise.</returns>
        bool WriteJson(object value, out object result);
    }
}
