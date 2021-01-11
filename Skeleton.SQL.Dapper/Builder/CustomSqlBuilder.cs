using Dapper.FluentMap;
using Dommel;
using System;

namespace Skeleton.SQL.Dapper.Builder
{
     public class CustomSqlBuilder : SqlServerSqlBuilder
    {
        public override string BuildInsert(Type type, string tableName, string[] columnNames, string[] paramNames)
        {
            if (!FluentMapper.EntityMaps.ContainsKey(type))
                return base.BuildInsert(type, tableName, columnNames, paramNames);

            return $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) OUTPUT INSERTED.* VALUES ({string.Join(", ", paramNames)}) ";
        }
    }
}
