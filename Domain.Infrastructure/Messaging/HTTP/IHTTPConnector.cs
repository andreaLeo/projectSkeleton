using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Infrastructure.Messaging.HTTP
{
     public interface IHttpConnector : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        NetworkCredential Credentials { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Uri Uri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeoutInSec"></param>
        /// <param name="partialGetBytes"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Get<T>(string request, double timeoutInSec = 30.0, int partialGetBytes = 0, CancellationToken token = default(CancellationToken)) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpContent"></param>
        /// <param name="timeoutInSec"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Post<T>(string request, HttpContent httpContent, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken)) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpContent"></param>
        /// <param name="timeoutInSec"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Put<T>(string request, HttpContent httpContent, double timeoutInSec = 300.0, CancellationToken token = default(CancellationToken)) where T : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeoutInSec"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Delete<T>(string request, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken)) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeoutInSec"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Head<T>(string request, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken)) where T : class, new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeoutInSec"></param>
        /// <returns></returns>
        Task<bool> Ping(string request, double timeoutInSec = 30.0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="destinationFilename"></param>
        /// <param name="downloadInProgress"></param>
        /// <returns></returns>
        Task Download(string request, string destinationFilename,
            IProgress<DownloadProgressReport> downloadInProgress);

        /// <summary>
        ///  Action call before each http request 
        /// param1 = request url send by user
        /// param2 = http header that will be associate with the request
        ///  </summary>
        Action<HttpMethod, string, HttpRequestHeaders> AddHttpRequestHeaderValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool DisposeHandShakeHandlerEachCall { get; set; }
    }
}
