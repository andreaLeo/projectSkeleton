using Dapper;
using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Storage.SQL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace Skeleton.SQL.Dapper
{
    public class DapperReader : DapperConnector, ISQLReader
    {
        private readonly ILogger _logger;
        public DapperReader(ILoggerFactory loggerFactory,
            IDependencyResolver resolver) : base(loggerFactory, resolver)
        {
            _logger = loggerFactory.CreateLogger<DapperReader>();
        }


        public T GetUnique<T>(string query, IsolationLevel isolationLevel = IsolationLevel.Snapshot) => ExecuteInTransaction(isolationLevel, (connection, transaction) => connection.QuerySingle<T>(query, null, transaction), query);

        public IEnumerable<T> GetAll<T>(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
            where T : class
         => GetByParameters<T>(null, isolationLevel);

        public IEnumerable<T> Get<T>(string query, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot) => ExecuteInTransaction(isolationLevel, (connection, transaction) => connection.Query<T>(query, param, transaction), query);

        public IEnumerable<T> GetByStoredProcedure<T>(string storedProcedureName, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot, int? commandTimeoutSec = null)
            where T : class => ExecuteInTransaction(isolationLevel, (connection, transaction) => connection.Query<T>(storedProcedureName, param, transaction, true, commandTimeoutSec, CommandType.StoredProcedure), $"{nameof(GetByStoredProcedure)}<{typeof(T).Name}> - {storedProcedureName} {param}");

        public IEnumerable<T> GetByQueryHierarchicalObject<T>(string query, Type[] typesInQueryResult, Func<object[], T> map,
            string[] splitParam = null, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot,
            int? commandTimeoutSec = null)
            where T : class => GetHierarchicalObject(query, typesInQueryResult, map, splitParam, param, isolationLevel, commandTimeoutSec);
        
        public IEnumerable<T> GetByStoredProcedureHierarchicalObject<T>(string storedProcedureName, Type[] typesInQueryResult, Func<object[], T> map,
            string[] splitParam = null, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot,
            int? commandTimeoutSec = null)
        where T : class => GetHierarchicalObject(storedProcedureName, typesInQueryResult, map, splitParam, param, isolationLevel, commandTimeoutSec, CommandType.StoredProcedure);
        
        private IEnumerable<T> GetHierarchicalObject<T>(string query, Type[] typesInQueryResult,
            Func<object[], T> map, string[] splitParam = null, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot,
            int? commandTimeoutSec = null, CommandType? commandType = null)
            where T : class
        {
            string splitOn = splitParam == null ? Builder.BuildSplitOn(typesInQueryResult) : string.Join(",", splitParam);
            IEnumerable<T> ret = ExecuteInTransaction(isolationLevel,
                (connection, transaction) =>
                {
                    try
                    {
                        return connection.Query<T>(query, typesInQueryResult, map, param, transaction,
                            true, splitOn, commandTimeoutSec, commandType);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (e.Message == "No columns were selected" && e.Source == "Dapper")
                            return new List<T>();
                        throw;
                    }

                },
                $"{nameof(GetHierarchicalObject)}<{typeof(T).Name}> - {query} {param}");
            return ret;
        }

        public IEnumerable<T> GetByParameters<T>(QueryParameter param, IsolationLevel isolationLevel = IsolationLevel.Snapshot)
            where T : class
        {
            string query = Builder.BuildGetQuery(typeof(T), param);
            IEnumerable<T> ret = ExecuteInTransaction(isolationLevel, (connection, transaction) =>
            {
                CheckTempTableCreation(connection, transaction, query, param?.Param);
                return connection.Query<T>(query, param?.Param, transaction);
            }, $"{nameof(GetByParameters)}<{typeof(T).Name}> - {query} {param?.Param}");

            return ret;
        }
    }
}
