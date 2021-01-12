using Domain.Infrastructure.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq.HttpApi
{
    class RabbitMqJsonContractResolver : IJsonCustomContract
    {
        public Func<string, string> DictionaryKeyResolver => s => s;

        public bool UseCamelCaseNamingStrategy => throw new NotImplementedException();

        public string ChangePropertyName(MemberInfo member, Type declaringType)
        {
            throw new NotImplementedException();
        }

        public Func<string, string> DictionaryKeyResolverByType(Type objectType)
        {
            throw new NotImplementedException();
        }

        public bool IgnoreProperty(Type declaringType, MemberInfo member)
        {
            throw new NotImplementedException();
        }

        public string ResolvePropertyName(string propertyName) => Regex.Replace(propertyName, "([a-z])([A-Z])", "$1_$2").ToLower();

        public bool ShouldCreateProperty(MemberInfo member, object instance)
        {
            throw new NotImplementedException();
        }

        public IJsonValueProvider ValueProvider(MemberInfo member, Type instanceType)
        {
            throw new NotImplementedException();
        }
    }
}
