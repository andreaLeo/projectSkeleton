using Domain.Infrastructure.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Skeleton.Serialization.NewtonSoft.Contract
{
     internal class CustomContractWrapper<T> : DefaultContractResolver
        where T : class, IJsonCustomContract, new()
    {
        private readonly T _customJsonContract = new T();

        public CustomContractWrapper()
        {
            if (_customJsonContract.UseCamelCaseNamingStrategy)
            {
                NamingStrategy = new CamelCaseNamingStrategy();
            }
        }

        /// <inheritdoc />
        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedValue = _customJsonContract.ResolvePropertyName(propertyName);
            return string.IsNullOrEmpty(resolvedValue) ? base.ResolvePropertyName(propertyName) : resolvedValue;
        }
   
        /// <inheritdoc />
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);
            var func = _customJsonContract.DictionaryKeyResolverByType(objectType);
            if (func != null)
            {
                contract.DictionaryKeyResolver = func;
            }

            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            property.Ignored = _customJsonContract.IgnoreProperty(property.DeclaringType, member);
            if (property.Ignored)
            {
                return property;
            }
            property.ShouldSerialize = instance => _customJsonContract.ShouldCreateProperty(member, instance);
            string propertyName =  _customJsonContract.ChangePropertyName(member, property.DeclaringType);
            if (!string.IsNullOrEmpty(propertyName))
            {
                property.PropertyName = propertyName;
            }
            
            var valueProvider = _customJsonContract.ValueProvider(member, property.DeclaringType);
            if (valueProvider != null)
            {
                property.ValueProvider = new ValueProviderWrapper(valueProvider);
            }
            return property;
        }

        private class ValueProviderWrapper : IValueProvider
        {
            private readonly IJsonValueProvider _valueProvider;

            internal ValueProviderWrapper(IJsonValueProvider customValueProvider)
            {
                _valueProvider = customValueProvider;
            }
            public object GetValue(object target) => _valueProvider.GetValue(target);


            public void SetValue(object target, object value) => _valueProvider.SetValue(target, value);
        }
    }
}
