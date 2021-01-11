using Domain.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Domain.Infrastructure.Reflection
{
    public interface IObjectReflectionAccessService : ISkeletonService
    {
        bool GetTypeByName(string typeName, out Type type);

        /// <summary>
        /// Gets read accessors for the given <paramref name="type"/>.
        /// </summary>
       
        IReadOnlyDictionary<string, Func<object, object>> GetReadAccess(Type type);

        /// <summary>
        /// Gets write accessors for the given <paramref name="type"/>.
        /// </summary>
        
        IReadOnlyDictionary<string, Action<object, object>> GetWriteAccess(Type type);

        /// <summary>
        /// Creates a new instance of the given <paramref name="type"/>.
        /// </summary>
       
        object Construct(Type type);

        /// <summary>
        /// Gets the value of the property <paramref name="propertyName"/> of the given <paramref name="target"/> instance.
        /// </summary>
        /// <returns>True if value was get, false otherwise.</returns>
        bool Read(object target, string propertyName, out object result);

        /// <summary>
        /// Sets the value of the property <paramref name="propertyName"/> of the given <paramref name="target"/> instance.
        /// </summary>
        /// <returns>True if value was set, false otherwise.</returns>
        bool Write(object target, string propertyName, object value);

        ///// <summary>
        ///// Indicates if the given <paramref name="type"/> is a leaf type or not.
        ///// </summary>
        //[Pure]
        //bool IsLeafType( Type type);

        /// <summary>
        /// Registers the given <paramref name="type"/>.
        /// </summary>
        void RegisterType( Type type);

        /// <summary>
        /// Registers type from the given provider.
        /// </summary>
        void RegisterTypeProvider(ISerializationTypeProvider provider);

        /// <summary/>
        PropertyDescriptorCollection GetPropertiesDescriptors(Type type);

        /// <summary>
        /// Check if given type is registered
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegister(Type type);
    }
}
