using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq.HttpApi.Model
{
    internal class Permission
    {
        public const string denyAll = "^$";
        public const string allowAll = ".*";

        public string Configure { get; set; } = allowAll;
        public string Write { get; set; } = allowAll;
        public string Read { get; set; } = allowAll;
    }

    internal class VHost
    {
        public string Name { get; set; }
    }
}
