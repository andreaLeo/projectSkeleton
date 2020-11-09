using System;
using System.Collections.Generic;
using System.Data;

namespace Domain.Infrastructure.Storage.SQL
{
    public interface ISQLReader
    {
        T GetUnique<T>(string query, IsolationLevel isolationLevel = IsolationLevel.Snapshot);
         IEnumerable<T> GetAll<T>(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
            where T : class;
         IEnumerable<T> Get<T>(string query, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot);

           IEnumerable<T> GetByStoredProcedure<T>(string storedProcedureName, object param = null,
            IsolationLevel isolationLevel = IsolationLevel.Snapshot, int? commandTimeoutSec = null)
            where T : class;

         IEnumerable<T> GetByParameters<T>(QueryParameter param, IsolationLevel isolationLevel = IsolationLevel.Snapshot)
            where T : class;

        IEnumerable<T> GetByStoredProcedureHierarchicalObject<T>(string storedProcedureName, Type[] typesInQueryResult,
            Func<object[], T> map,
            string[] splitParam = null, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot,
            int? commandTimeoutSec = null)
            where T : class;

         IEnumerable<T> GetByQueryHierarchicalObject<T>(string query, Type[] typesInQueryResult,
            Func<object[], T> map,
            string[] splitParam = null, object param = null, IsolationLevel isolationLevel = IsolationLevel.Snapshot,
            int? commandTimeoutSec = null)
            where T : class;
    }
}
