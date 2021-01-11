using Dapper.FluentMap.Dommel.Mapping;
using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.SQL.Dapper.Mapper
{
   internal interface ICustomDapperMapper : IEntityMap
    {
        void Map();
    }

    public abstract class DapperMapperBase<T> : DommelEntityMap<T>, ICustomDapperMapper
        where T : class
    {
        protected const string DEFAULT_SHEMA_NAME = "dbo";

     
        public void Map()
        {
            foreach (DommelPropertyMap propertyMap in PropertyMaps)
            {

            }
        }
    }
}
