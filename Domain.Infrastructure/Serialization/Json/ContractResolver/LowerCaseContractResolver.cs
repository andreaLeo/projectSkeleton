using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Domain.Infrastructure.Serialization.Json.ContractResolver
{
    /// <summary />
    public class LowerCaseContractResolver : IJsonCustomContract
    {
        /// <summary />
        public string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }

        /// <summary />
        public bool ShouldCreateProperty(MemberInfo member, object instance)
        {
            throw new NotImplementedException();
        }

        /// <summary />
        public IJsonValueProvider ValueProvider(MemberInfo member, Type instanceType)
        {
            throw new NotImplementedException();
        }
        /// <summary />
        public bool IgnoreProperty(Type declaringType, MemberInfo member)
        {
            throw new NotImplementedException();
        }
        /// <summary />
        public string ChangePropertyName(MemberInfo member, Type declaringType)
        {
            throw new NotImplementedException();
        }
        /// <summary />
        public Func<string, string> DictionaryKeyResolverByType(Type objectType)
        {
            throw new NotImplementedException();
        }

        /// <summary />
        public Func<string, string> DictionaryKeyResolver { get; } = null;

        /// <summary />
        public bool UseCamelCaseNamingStrategy => false;
    }
}
