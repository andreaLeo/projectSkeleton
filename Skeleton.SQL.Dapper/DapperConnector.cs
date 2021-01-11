using Dapper;
using Domain.Infrastructure.DependencyInjection;
using Domain.Infrastructure.Serialization.Json;
using Domain.Infrastructure.Storage.SQL;
using Microsoft.Extensions.Logging;
using Skeleton.SQL.Dapper.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Skeleton.SQL.Dapper
{
    public abstract class DapperConnector
    {
        private readonly ILogger _logger;
        protected DapperConnector(ILoggerFactory loggerFactory,
            IDependencyResolver resolver)
        {
            _logger = loggerFactory.CreateLogger(GetType());

            Builder = resolver.Resolve<CustomQueryBuilder>();
            var serializer = resolver.Resolve<IJsonSerializer>();

            var config = serializer.FileToObject<SQLConfiguration>(Path.Combine(Directory.GetCurrentDirectory(), "sql.json"));
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                UserID = config.UserID,
                Password = config.Password,
                InitialCatalog = config.InitialCatalog,
                DataSource = config.DataSource,
            };
            ConnectionString = builder.ToString();
            _logger.LogInformation($"ConnectionString: {ConnectionString} ...");
        }

        protected string ConnectionString { get; private set; }
        protected CustomQueryBuilder Builder { get; private set; }

        protected T ExecuteInTransaction<T>(IsolationLevel isolationLevel, Func<IDbConnection, IDbTransaction, T> toCall, string query)
        {
            T ret = default(T);
            try
            {
                using (IDbConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (IDbTransaction transaction = connection.BeginTransaction(isolationLevel))
                    {
                        try
                        {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            ret = toCall(connection, transaction);
                            transaction.Commit();
                            sw.Stop();
                            _logger.LogInformation($"Execute query: {query} in ({sw.ElapsedMilliseconds} ms).");
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Query: {query} - Try to rollback.");
                            try
                            {
                                transaction.Rollback();
                                connection.Close();
                                _logger.LogInformation("Rollback succeed");
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError(exception, "Rollback failed.");
                                connection.Close();
                                throw;
                            }

                            throw;
                        }

                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sql Error: ");
            }

            return ret;
        }

        protected void CheckTempTableCreation(IDbConnection connection, IDbTransaction transaction, string query, object param)
        {
            if (query.Contains(CustomQueryBuilder.SQL_INNER_JOIN_TO_TEMP_TABLE) && param is List<KeyValuePair<string, object>>)
            {
                List<KeyValuePair<string, object>> orig = (List<KeyValuePair<string, object>>)param;
                List<KeyValuePair<string, object>> tmpTable = orig.Where(pair => pair.Key.StartsWith("#")).ToList();

                foreach (KeyValuePair<string, object> obj in tmpTable)
                {
                    Array src = obj.Value as Array;
                    connection.Execute($"CREATE TABLE {obj.Key}(Id {GetStrDbType(src.GetValue(0).GetType())} not null primary key);", null, transaction);
                    connection.Execute($"INSERT INTO {obj.Key} VALUES (@Id);", src.Cast<object>().Distinct().Select(o => new { Id = o }), transaction);
                    _logger.LogInformation("Temp table creation.");
                    orig.Remove(obj);
                }
            }
        }

        private string GetStrDbType(Type type)
        {
            SqlParameter parameter = new SqlParameter();
            TypeConverter converter = TypeDescriptor.GetConverter(parameter.DbType);
            parameter.DbType = (DbType)converter.ConvertFrom(type.Name);
            return parameter.SqlDbType == SqlDbType.NVarChar ? $"{SqlDbType.NVarChar}(max)".ToLower(CultureInfo.InvariantCulture) : parameter.SqlDbType.ToString().ToLower(CultureInfo.InvariantCulture);
        }

    }
}
