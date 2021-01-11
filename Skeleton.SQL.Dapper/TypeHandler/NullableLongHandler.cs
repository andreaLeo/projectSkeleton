using Dapper;
using System;
using System.Data;

namespace Skeleton.SQL.Dapper.TypeHandler
{
     public class NullableLongHandler : SqlMapper.TypeHandler<long?>
    {
        public override void SetValue(IDbDataParameter parameter, long? value) => parameter.Value = value.HasValue ? value.Value : DBNull.Value;
        public override long? Parse(object value) => value == null || value is DBNull ? null : Convert.ToInt64(value);
    }
}
