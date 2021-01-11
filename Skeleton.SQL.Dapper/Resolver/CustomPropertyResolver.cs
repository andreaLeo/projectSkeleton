using Dapper.FluentMap;
using Dapper.FluentMap.Dommel.Mapping;
using Dapper.FluentMap.Dommel.Resolvers;
using Dapper.FluentMap.Mapping;
using Dommel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.SQL.Dapper.Resolver
{
    public class CustomPropertyResolver : DommelPropertyResolver
    {
        public override IEnumerable<ColumnPropertyInfo> ResolveProperties(Type type)
        {
            IEntityMap entityMap;
            if (FluentMapper.EntityMaps.TryGetValue(type, out entityMap))
            {
                foreach (var property in FilterComplexTypes(type.GetProperties()))
                {
                    // Determine whether the property should be ignored.
                    var propertyMap = entityMap.PropertyMaps.FirstOrDefault(p => p.PropertyInfo.Name == property.Name);
                    if (propertyMap == null || !propertyMap.Ignored)
                    {
                        var dommelPropertyMap = propertyMap as DommelPropertyMap;
                        if (dommelPropertyMap != null)
                        {
                            yield return dommelPropertyMap.GeneratedOption != System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None
                                ? new ColumnPropertyInfo(property, generatedOption: dommelPropertyMap.GeneratedOption)
                                : new ColumnPropertyInfo(property, isKey: dommelPropertyMap.Key);
                        }
                        else
                        {
                            yield return new ColumnPropertyInfo(property);
                        }
                    }
                }
            }
            else
            {
                base.ResolveProperties(type);
            }
        }
    }
}
