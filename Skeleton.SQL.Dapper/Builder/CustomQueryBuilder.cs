using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Dommel;
using Dapper.FluentMap.Dommel.Mapping;
using Dapper.FluentMap.Mapping;
using Domain.Infrastructure.Storage.SQL;
using Dommel;
using Microsoft.Extensions.Logging;
using Skeleton.SQL.Dapper.Mapper;
using Skeleton.SQL.Dapper.Resolver;
using Skeleton.SQL.Dapper.TypeHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.SQL.Dapper.Builder
{
    public class CustomQueryBuilder
    {
        private readonly Dictionary<QueryOperator, string> _queryOperator = new Dictionary<QueryOperator, string>();
        private readonly ILogger _logger;

        private const int SQL_IN_LIMIT = 1000;
        public const string SQL_WHERE = "WHERE";
        public const string SQL_INNER_JOIN_TO_TEMP_TABLE = "INNER JOIN #";


        private readonly Dictionary<Type, Func<object, string>> _specificTypeConverter = new Dictionary<Type, Func<object, string>>();

        public CustomQueryBuilder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            GetEnumDescription();
            if (FluentMapper.EntityMaps.Count == 0)
                CreateMapper();
            DommelMapper.LogReceived = str => _logger.LogInformation(str);
        }


        public string TableNameMapper(Type toGenerate)
        {
            if (!FluentMapper.EntityMaps.ContainsKey(toGenerate))
                return toGenerate.Name;
            IDommelEntityMap entityMap = (IDommelEntityMap)FluentMapper.EntityMaps[toGenerate];
            return entityMap.TableName;
        }

        private void AddMap<TMap, T>(FluentMapConfiguration mapConfiguration)
            where TMap : DapperMapperBase<T>, new()
            where T : class
        {
            TMap map = new TMap();
            mapConfiguration.AddMap(map);
        }

        private void CreateMapper()
        {
            FluentMapper.Initialize(configuration =>
            {
                
                configuration.ForDommel();
                DommelMapper.SetPropertyResolver(new CustomPropertyResolver());
            });

            foreach (KeyValuePair<Type, IEntityMap> key in FluentMapper.EntityMaps)
            {
                ICustomDapperMapper customMapper = key.Value as ICustomDapperMapper;
                if (customMapper != null)
                {
                    customMapper.Map();
                   
                }
                
            }

            SqlMapper.AddTypeHandler(new NullableLongHandler());
        }

        private void GetEnumDescription()
        {
            foreach (QueryOperator enumValue in Enum.GetValues(typeof(QueryOperator)))
            {
                FieldInfo fi = typeof(QueryOperator).GetField(enumValue.ToString());

                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                    _queryOperator[enumValue] = attributes[0].Description;
                else
                    _queryOperator[enumValue] = enumValue.ToString();
            }

        }

        public string BuildSplitOn(IEnumerable<Type> types)
        {
            List<string> splitOnList = new List<string>();
            foreach (Type type in types)
            {
                IEntityMap map;
                if (FluentMapper.EntityMaps.TryGetValue(type, out map))
                {
                    foreach (DommelPropertyMap propertyMap in map.PropertyMaps)
                    {
                        if (propertyMap.Key || propertyMap.Identity)
                            splitOnList.Add(propertyMap.PropertyInfo.Name);
                    }
                }
                else
                {
                    _logger.LogWarning($"No mapper found for type: {type.Name}");
                }
            }

            return splitOnList.Count != 0 ? string.Join(",", splitOnList) : "Id";
        }

        public string BuildDeleteQuery(Type toGenerate, QueryParameter parameters)
        {
            string tableName = TableNameMapper(toGenerate);

            return tableName != null ? BuildQuery(parameters, $"DELETE src FROM {tableName} src") : null;
        }

        public string BuildGetQuery(Type toGenerate, QueryParameter parameters)
        {
            string tableName = TableNameMapper(toGenerate);

            return tableName != null ? BuildQuery(parameters, $"SELECT * FROM {tableName} src") : null;
        }

        private string BuildQuery(QueryParameter parameters, string queryAction)
        {
            if (parameters == null)
                return queryAction;
            StringBuilder queryBuilder = new StringBuilder();

            List<KeyValuePair<string, object>> parametersValue = new List<KeyValuePair<string, object>>();

            bool isMultiParam = false;
            Type paramType = parameters.Param.GetType();
            if (typeof(IEnumerable<QueryParameter>).IsAssignableFrom(paramType))
            {
                isMultiParam = true;
                IEnumerable<QueryParameter> multiParam = (IEnumerable<QueryParameter>)parameters.Param;
                foreach (QueryParameter param in multiParam.ToList())
                {
                    Type subParamType = param.Param.GetType();
                    GetParamQuery(queryBuilder, subParamType, param.Operator, param.Param, parametersValue, isMultiParam);
                }
            }
            else
            {
                GetParamQuery(queryBuilder, paramType, parameters.Operator, parameters.Param, parametersValue, isMultiParam);
            }
            if (parametersValue.Count != 0 && (isMultiParam || parametersValue.Any(pair => pair.Key.StartsWith("#"))))
                parameters.Param = parametersValue;
            queryBuilder.Insert(0, queryAction);
            return queryBuilder.ToString();
        }


        private void GetParamQuery(StringBuilder queryBuilder, Type paramType, QueryOperator queryOperator, object param, List<KeyValuePair<string, object>> paramValue, bool isMultiParam)
        {
            foreach (PropertyInfo piParam in paramType.GetProperties())
            {
                if (queryOperator == QueryOperator.In && SplitInTempDb(queryBuilder, param, piParam, paramValue))
                    continue;

                queryBuilder.Append(paramValue.Count != 0
                    ? $" {_queryOperator[QueryOperator.And]} "
                    : $" {SQL_WHERE} ");

                string paramName = $"{piParam.Name}";

                if (isMultiParam)
                {
                    paramName = $"{piParam.Name}_{paramValue.Count}";
                    queryBuilder.Append($"{piParam.Name} {_queryOperator[queryOperator]} @{paramName}");
                }
                else
                {
                    queryBuilder.Append($"{piParam.Name} {_queryOperator[queryOperator]} @{piParam.Name}");
                }
                object p = piParam.GetValue(param);
                paramValue.Add(new KeyValuePair<string, object>(paramName, p));
            }
        }


        private bool SplitInTempDb(StringBuilder queryBuilder, object param, PropertyInfo piParam,
            List<KeyValuePair<string, object>> paramValue)
        {
          
            object p = piParam.GetValue(param);
            Array src = p as Array;
            if (src == null || src.Length <= SQL_IN_LIMIT)
                return false;

            string tablename = $"#{piParam.Name}_{paramValue.Count}";
            string tableAlias = $"tmp_{paramValue.Count}";
            queryBuilder.Insert(0, $" INNER JOIN {tablename} {tableAlias} ON {tableAlias}.Id = src.{piParam.Name} ");
             
            paramValue.Add(new KeyValuePair<string, object>(tablename, src));
            return true;

        }
    }
}
