using Domain.Infrastructure.Messaging.HTTP;
using Domain.Infrastructure.Serialization.Json;
using Skeleton.Amqp.RabbitMq.Configuration;
using Skeleton.Amqp.RabbitMq.HttpApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Skeleton.Amqp.RabbitMq.HttpApi
{
    public class VHostManager
    {
        private readonly Dictionary<BrokerConfiguration, HashSet<string>> _vhosts = new Dictionary<BrokerConfiguration, HashSet<string>>();
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IHttpConnector _httpConnector;

        public VHostManager(IJsonSerializer serializer, 
            IHttpConnector httpConnector)
        {
            _jsonSerializer = serializer;
            _httpConnector = httpConnector;
          //  _jsonSerializer.AddCustomContractResolver<RabbitContractResolver>();
        }

     
        public void AddHost(BrokerConfiguration configuration, string vHostName)
        {
            if (!_vhosts.ContainsKey(configuration))
                _vhosts[configuration] = new HashSet<string>();

            _vhosts[configuration].Add(vHostName);
        }

//         public void CreateVHost(BrokerConfiguration configuration, Assembly callerAssembly)
//         {
//             AssemblyBuildInfoAttribute buildInfo = BuildHelper.GetBuildInfo(callerAssembly);
//             if (buildInfo != null)
//             {
//                 string vHostTestName = $@"{configuration.Vhost}_{buildInfo.BuildDate}_{buildInfo.BranchName.Replace('/', '_')}_{buildInfo.BuildNumber}";
// #if DEBUG
//                 vHostTestName = $@"{configuration.Vhost}_{Environment.MachineName}_{Environment.UserName}";
// #endif
//                 configuration.Vhost = CreateVHost(configuration, vHostTestName);
//             }
//         }

        private T HTTPCall<T>(BrokerConfiguration cfg, Func<T> httpAction)
        {
            _httpConnector.Credentials = new NetworkCredential { UserName = cfg.UserName, Password = cfg.Password };
            _httpConnector.Uri = new Uri($"http://{cfg.HostName}:1{cfg.Port}/");
            return httpAction();
        }

        private async Task<T> CallGetRabbitMqApi<T>(BrokerConfiguration cfg, string call)
            where T : class
        {
            return await HTTPCall(cfg, async () => await _httpConnector.Get<T>(call));
        }

        private void CallPutRabbitMqApi(BrokerConfiguration cfg, string call, object body = null)
        {
            HTTPCall(cfg, () =>
            {
                var content = new StringContent(body != null ? _jsonSerializer.SerializeObject(body) : "", Encoding.UTF8, "application/json");
                return _httpConnector.Put<string>(call, content);
            });
        }

        private void CallDeleteRabbitMqApi(BrokerConfiguration cfg, string call) => HTTPCall(cfg, () => _httpConnector.Delete<string>(call));


        private IEnumerable<VHost> GetVHost(BrokerConfiguration cfg) => CallGetRabbitMqApi<IEnumerable<VHost>>(cfg, "api/vhosts").Result;

        private string CreateVHost(BrokerConfiguration configuration, string vHostName)
        {
            var vhosts = GetVHost(configuration);
            string httpVHost = vHostName.Replace(@"/", @"%2f");
            VHost vhost = vhosts.FirstOrDefault(vh => vh.Name == vHostName);
            if (vhost == null)
            {
                CallPutRabbitMqApi(configuration, $"api/vhosts/{httpVHost}");
                CallPutRabbitMqApi(configuration, $"api/permissions/{httpVHost}/{configuration.UserName}", new Permission());
                if (!_vhosts.ContainsKey(configuration))
                    _vhosts[configuration] = new HashSet<string>();

                _vhosts[configuration].Add(vHostName);
            }
            return vHostName;
        }

        public void DeleteVHost()
        {
            foreach (KeyValuePair<BrokerConfiguration, HashSet<string>> obj in _vhosts)
            {
                foreach (string vhosts in obj.Value)
                {
                    DeleteVHost(obj.Key, vhosts);
                }
            }
            _vhosts.Clear();
        }

        private void DeleteVHost(BrokerConfiguration configuration, string vHostName)
        {
            var vhosts = GetVHost(configuration);
            string httpVHost = vHostName.Replace(@"/", @"%2f");
            VHost vhost = vhosts.FirstOrDefault(vh => vh.Name == vHostName);
            if (vhost == null)
                return;
            CallDeleteRabbitMqApi(configuration, $"api/vhosts/{httpVHost}");
        }
    }
}
