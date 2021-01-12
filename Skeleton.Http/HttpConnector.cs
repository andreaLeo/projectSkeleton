using Domain.Infrastructure.Messaging.HTTP;
using Skeleton.Http.Helper;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Skeleton.Http
{
     public class HttpConnector : IHttpConnector
    {
        private readonly ConcurrentDictionary<object, IProgress<DownloadProgressReport>> _downloadMetaDataByFilename = new ConcurrentDictionary<object, IProgress<DownloadProgressReport>>();
        private HttpClientHandler _httpClientHandler;
        private NetworkCredential _networkCredential;


        private const string QUERY_FILE_KEY = "File";
        public NetworkCredential Credentials
        {
            get { return _networkCredential; }
            set
            {
                if (_networkCredential == value) return;

                _networkCredential = value;
                if (!DisposeHandShakeHandlerEachCall)
                    Dispose();
            }
        }

        public bool DisposeHandShakeHandlerEachCall { get; set; } = true;

        public Uri Uri { get; set; }

        private async Task<T> HttpCall<T>(Func<HttpClient, string, Task<T>> httpAction, HttpMethod httpMethod, string request)
        {
            // if (Uri == null)
            //     throw new ArgumentNullException(nameof(Uri));
            HttpMessageHandler httpMessageHandler;

            if (!DisposeHandShakeHandlerEachCall)
            {
                if (_httpClientHandler == null)
                    _httpClientHandler = CreateHandler();
                httpMessageHandler = _httpClientHandler;
            }
            else
            {
                httpMessageHandler = CreateHandler();
            }


            using (var client = new HttpClient(httpMessageHandler, DisposeHandShakeHandlerEachCall)
            {
                BaseAddress = Uri,
            })
            {
                AddHttpRequestHeaderValue?.Invoke(httpMethod, request, client.DefaultRequestHeaders);

                T ret = await httpAction(client, request);
                if (DisposeHandShakeHandlerEachCall)
                    Dispose();
                return ret;
            }
        }

        public async Task<T> Get<T>(string request, double timeoutInSec = 30.0, int partialGetBytes = 0, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return await HttpCall(async (client, uri) =>
            {
                var taskResponse = await client.GetAsync(uri, token);
                if (partialGetBytes > 0)
                {
                    return await PartialReadHttpResponse<T>(taskResponse, request, partialGetBytes, timeoutInSec);
                }
                return await ReadHttpResponse<T>(taskResponse, $"{client.BaseAddress}{uri}", timeoutInSec);
            }, HttpMethod.Get, request);
        }

        public async Task<T> Post<T>(string request, HttpContent httpContent, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return await HttpCall(async (client, uri) =>
            {
                HttpResponseMessage response = await client.PostAsync(uri, httpContent, token);
                return await ReadHttpResponse<T>(response, $"{client.BaseAddress}{uri}", timeoutInSec);
            }, HttpMethod.Post, request);
        }

        public async Task<T> Put<T>(string request, HttpContent httpContent, double timeoutInSec = 300.0, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return await HttpCall(async (client, uri) =>
            {
                HttpResponseMessage response = await client.PutAsync(uri, httpContent, token);
                return await ReadHttpResponse<T>(response, $"{client.BaseAddress}{uri}", timeoutInSec);
            }, HttpMethod.Put, request);
        }

        public async Task<T> Delete<T>(string request, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return await HttpCall(async (client, uri) =>
            {
                HttpResponseMessage response = await client.DeleteAsync(uri, token);
                return await ReadHttpResponse<T>(response, $"{client.BaseAddress}{uri}", timeoutInSec);
            }, HttpMethod.Delete, request);
        }

        public async Task<T> Head<T>(string request, double timeoutInSec = 30.0, CancellationToken token = default(CancellationToken))
            where T : class, new()
        {
            return await HttpCall(async (client, uri) =>
            {
                HttpRequestMessage headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                HttpResponseMessage response = await client.SendAsync(headRequest, token);
//                bool wait = response.Wait(TimeSpan.FromSeconds(timeoutInSec));
                if (response.IsSuccessStatusCode)
                {
                    T obj = new T();
                    HttpMetadataHelper.SetObjectValuesFromHeaders(obj, response.Headers);
                    HttpMetadataHelper.SetObjectValuesFromHeaders(obj, response.Content.Headers);
                    return obj;
                }

                throw new HttpRequestException(response.ToString());
            }, HttpMethod.Head, request);
        }

        public async Task<bool> Ping(string request, double timeoutInSec = 30)
        {
            if (Uri.IsWellFormedUriString(request, UriKind.Absolute))
            {
                if (!string.IsNullOrEmpty(request))
                    Uri = new Uri(request);
                return await HttpCall(async (client, uri) =>
                {
                    bool status = false;
                    try
                    {
                        HttpRequestMessage headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                        HttpResponseMessage response = await client.SendAsync(headRequest);
                       // bool wait = response.Wait(TimeSpan.FromSeconds(timeoutInSec));
                        if (response.IsSuccessStatusCode)
                        {
                            status = true;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    return status;
                }, HttpMethod.Head, request);
            }

            bool pingable = false;
            try
            {
                using (Ping pinger = new Ping())
                {
                    PingReply reply = await pinger.SendPingAsync(request, (int)TimeSpan.FromSeconds(timeoutInSec).TotalMilliseconds);
                    pingable = reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
        }

        public async Task Download(string requestUri, string destinationFilename, IProgress<DownloadProgressReport> downloadInProgress = null)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            using (WebClient client = new WebClient
            {
                BaseAddress = Uri.ToString(),
            })
            {
                if (Credentials != null)
                    client.Credentials = Credentials;

                if (downloadInProgress != null)
                {
                    client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
                    client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
                }

                _downloadMetaDataByFilename.TryAdd(destinationFilename, downloadInProgress);

                //Uri downloadUri;
                //if (!Uri.TryCreate(requestUri, UriKind.Absolute, out downloadUri))
                //    Uri.TryCreate(Uri, requestUri, out downloadUri);
                client.QueryString[QUERY_FILE_KEY] = destinationFilename;
                await client.DownloadFileTaskAsync(requestUri, destinationFilename);
            }
        }

        private async Task<T> PartialReadHttpResponse<T>(HttpResponseMessage taskResponse, string requestUri, int partialGetBytes, double timeoutInSec)
            where T : class
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                byte[] buffer = new byte[partialGetBytes];
                using (Stream stream = await taskResponse.Content.ReadAsStreamAsync())
                {
                    while (stream.Read(buffer, 0, partialGetBytes - 1) < partialGetBytes - 1) ;
                }
                return new MemoryStream(buffer) as T;
            }

            throw new HttpRequestException($"{taskResponse.Content.ReadAsStringAsync().Result} - Code: {taskResponse.StatusCode} - Request: {requestUri}");
        }

        private async Task<T> ReadHttpResponse<T>(HttpResponseMessage response, string requestUri, double timeoutInSec)
            where T : class
        {
            //  bool wait = queryTask.Wait(TimeSpan.FromSeconds(timeoutInSec));
            if (response.IsSuccessStatusCode)
            {
                T ret;

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    ret = await response.Content.ReadAsStreamAsync() as T;
                }
                else if (typeof(string).IsAssignableFrom(typeof(T)))
                {
                    ret = await response.Content.ReadAsStringAsync() as T;
                }
                else if (typeof(T) == typeof(byte[]))
                {
                    ret = await response.Content.ReadAsByteArrayAsync() as T;
                }
                else
                {
                    ret = await response.Content.ReadAsAsync<T>();
                }
                HttpMetadataHelper.SetObjectValuesFromHeaders(ret, response.Headers);
                HttpMetadataHelper.SetObjectValuesFromHeaders(ret, response.Content.Headers);
                return ret;
            }

            throw new HttpRequestException($"{response.Content.ReadAsStringAsync().Result} - Code: {response.StatusCode} - Request: {requestUri}");
        }

        public Action<HttpMethod, string, HttpRequestHeaders> AddHttpRequestHeaderValue { get; set; }


        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            WebClient client = (WebClient)sender;
            string filename = client.QueryString[QUERY_FILE_KEY];

            DownloadProgressReport report = new DownloadProgressReport
            {
                Filename = filename,
                BytesReceived = downloadProgressChangedEventArgs.BytesReceived,
                TotalBytesToReceive = downloadProgressChangedEventArgs.TotalBytesToReceive,
                PercentageComplete = downloadProgressChangedEventArgs.ProgressPercentage,
            };

            _downloadMetaDataByFilename[filename].Report(report);
        }

        private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            WebClient client = (WebClient)sender;
            IProgress<DownloadProgressReport> progress;
            string filename = client.QueryString[QUERY_FILE_KEY];
            _downloadMetaDataByFilename.TryRemove(filename, out progress);
            DownloadProgressReport report = new DownloadProgressReport
            {
                Filename = filename,
                PercentageComplete = 100,
            };

            client.DownloadFileCompleted -= ClientOnDownloadFileCompleted;
            client.DownloadProgressChanged -= ClientOnDownloadProgressChanged;
            progress.Report(report);
        }

        private HttpClientHandler CreateHandler()
        {
            if (_networkCredential == null)
                return new HttpClientHandler { UseDefaultCredentials = true };
            return new HttpClientHandler { Credentials = _networkCredential };
        }

        public void Dispose()
        {
            try
            {
                _httpClientHandler?.Dispose();
            }
            finally
            {
                _httpClientHandler = null;
            }
        }
    }
}
