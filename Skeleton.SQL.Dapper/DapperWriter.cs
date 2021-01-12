using Dapper;
using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Storage.SQL;
using Dommel;
using Microsoft.Extensions.Logging;
using Skeleton.SQL.Dapper.Builder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.SQL.Dapper
{
    public class DapperWriter : DapperConnector, ISQLWriter
    {
        private readonly ILogger _logger;
       
        public DapperWriter(ILoggerFactory loggerFactory, 
            IDependencyResolver dependencyResolver)
            : base(loggerFactory, dependencyResolver)
        {
            _logger = loggerFactory.CreateLogger<DapperWriter>();
            DommelMapper.AddSqlBuilder(typeof(SqlConnection), new CustomSqlBuilder());

        }

        public T Insert<T>(T toInsert, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, Action<object> resultCallback = null)
            where T : class
        {
            var result = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                return connection.Insert(toInsert, transaction);
            }, $"{nameof(Insert)}<{typeof(T).Name}>");

            resultCallback(result);

            return toInsert;
        }

        public IEnumerable<T> Insert<T>(IEnumerable<T> toInsert,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class
        {
            return ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                List<T> ret = new List<T>();
                foreach (T insert in toInsert)
                {
                    connection.Insert(insert, transaction);
                    ret.Add(insert);
                }
                return ret;
            }, $"{nameof(Insert)}<{typeof(T).Name}>");

        }

        public int Execute(string query, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            int? commandTimeoutSecond = null, CommandType? commandType = null)
        {
            int ret;
            if (isolationLevel == IsolationLevel.Unspecified)
            {
                using (IDbConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    ret = connection.Execute(query, null, null, commandTimeoutSecond, commandType);
                    sw.Stop();
                    _logger.LogInformation($"Execute query: {query} in ({sw.ElapsedMilliseconds} ms).");
                }
            }
            else
            {
                ret = ExecuteInTransaction(isolationLevel,
                    (connection, transaction) =>
                        connection.Execute(query, null, transaction, commandTimeoutSecond, commandType),
                    query);

            }

            return ret;
        }


        public bool Delete<T>(T toDelete, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class
        {
            bool ret = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                return connection.Delete<T>(toDelete, transaction);
            }, $"{nameof(Delete)}<{typeof(T).Name}>");

            return ret;
        }

        public bool Delete<T>(IEnumerable<T> toDelete, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class
        {
            bool ret = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                foreach (T obj in toDelete)
                {
                    connection.Delete<T>(obj, transaction);
                }

                return true;
            }, $"{nameof(Delete)}<{typeof(T).Name}>");

            return ret;
        }

        public int DeleteByParameters<T>(QueryParameter param, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null)
            where T : class
        {
            string query = Builder.BuildDeleteQuery(typeof(T), param);
            int ret = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                return connection.Execute(query, param.Param, transaction, commandTimeoutSecond);
            }, $"{nameof(DeleteByParameters)}<{typeof(T).Name}> - {query} {param.Param}");

            return ret;
        }

        public bool Update<T>(T toUpdate, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class
        {
            bool succeeded = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                return connection.Update<T>(toUpdate, transaction);
            }, $"{nameof(Update)}<{typeof(T).Name}>");

            return succeeded;
        }

        public bool Update<T>(IEnumerable<T> toUpdate, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class
        {
            bool succeeded = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                bool ret = true;
                foreach (T obj in toUpdate)
                {
                    ret &= connection.Update<T>(obj, transaction);
                }

                return ret;
            }, $"{nameof(Update)}<{typeof(T).Name}>");

            return succeeded;
        }

        public int ExecuteStoredProcedure(string storedProcedureName, object param = null, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeout = null)
        {
            int ret = ExecuteInTransaction(isolationLevel, (connection, transaction) => connection.Execute(storedProcedureName, param, transaction, commandTimeout, CommandType.StoredProcedure), $"{nameof(ExecuteStoredProcedure)} - {storedProcedureName} {param}");

            return ret;
        }
    }
}
