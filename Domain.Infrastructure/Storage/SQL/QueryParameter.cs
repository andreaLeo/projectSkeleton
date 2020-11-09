using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Domain.Infrastructure.Storage.SQL
{
    public class QueryParameter
    {
        /// <summary>
        /// 
        /// </summary>
        public QueryOperator Operator { get; set; } = QueryOperator.Equals;
        /// <summary>
        /// 
        /// </summary>
        public object Param { get; set; }
    }

    public enum QueryOperator
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("=")]
        Equals,
        /// <summary>
        /// 
        /// </summary>
        [Description(">")]
        GreaterThan,
        /// <summary>
        /// 
        /// </summary>
        [Description(">=")]
        GreaterOrEqualThan,
        /// <summary>
        /// 
        /// </summary>
        [Description("<")]
        LowerThan,
        /// <summary>
        /// 
        /// </summary>
        [Description("<=")]
        LowerOrEqualThan,
        /// <summary>
        /// 
        /// </summary>
        [Description("IN")]
        In,
        /// <summary>
        /// 
        /// </summary>
        [Description("AND")]
        And,
        /// <summary>
        /// 
        /// </summary>
        [Description("OR")]
        Or,
    }
}
