using System;
using System.Reflection;

namespace Domain.Infrastructure.Serialization.Json
{
    public interface IJsonCustomContract
    {
        
        /// <summary />
        string ResolvePropertyName(string propertyName);
        /// <summary />
        Func<string, string> DictionaryKeyResolver { get; }

         /// <summary />
        Func<string, string> DictionaryKeyResolverByType(Type objectType);
            

        /// <summary />
        IJsonValueProvider ValueProvider(MemberInfo member, Type instanceType);

        /// <summary />
        bool ShouldCreateProperty(MemberInfo member, object instance);
          /// <summary />
        bool IgnoreProperty(Type declaringType, MemberInfo member);
        /// <summary />
        bool UseCamelCaseNamingStrategy { get; }

        /// <summary />
        string ChangePropertyName(MemberInfo member, Type declaringType);
    }
}
