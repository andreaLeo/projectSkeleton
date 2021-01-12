using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Infrastructure.Messaging.HTTP.Attributes
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HttpHeader : System.Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
    }
}
