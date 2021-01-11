using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Domain.Infrastructure.Storage.SQL
{
    public interface ISQLWriter
    {
        T Insert<T>(T toInsert, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, Action<object> resultCallback = null)
          where T : class;

        IEnumerable<T> Insert<T>(IEnumerable<T> toInsert, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
          where T : class;

        bool Delete<T>(T toDelete, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
          where T : class;

        bool Delete<T>(IEnumerable<T> toDelete, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            where T : class;

        bool Update<T>(T toUpdate, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
         where T : class;

        bool Update<T>(IEnumerable<T> toUpdate, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
           where T : class;

        int DeleteByParameters<T>(QueryParameter param, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null)
           where T : class;

        int Execute(string query, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null, CommandType? commandType = null);

        IEnumerable<T> BulkInsert<T>(IEnumerable<T> toInsert,
           IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null, bool log = false)
           where T : class;

        void BulkUpdate<T>(IEnumerable<T> toUpdate,
         IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null, bool log = false)
         where T : class;

        void BulkDelete<T>(IEnumerable<T> toDelete,
           IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeoutSecond = null, bool log = false)
           where T : class;


        int ExecuteStoredProcedure(string storedProcedureName, object param = null,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int? commandTimeout = null);

    }
}
