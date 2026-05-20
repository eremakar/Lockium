using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Api.AspNetCore.Services
{
    public class HttpClientServiceBase
    {
        private readonly HttpClient httpClient;
        protected readonly ILogger<HttpClientServiceBase> logger;

        protected HttpClient Http => httpClient;

        public string AuthToken { get; set; }

        public string BaseUrl { get; set; }

        public HttpClientServiceBase(HttpClient httpClient, ILogger<HttpClientServiceBase> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task Authenticate(string url, string body,
            Func<HttpResponseMessage, Task<string>> parseToken,
            CancellationToken cancellationToken)
        {
            var authRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
                Content = new StringContent(body,
                    Encoding.UTF8, "application/json")
            };

            using (var response = await httpClient.SendAsync(authRequest, cancellationToken))
            {
                AuthToken = await parseToken(response);
            }
        }

        public void SetBasicAuth(string userName, string password)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
        }

        public async Task<T> SendAsAsync<T>(string url, object request,
            CancellationToken cancellationToken = default(CancellationToken),
            bool authorize = false)
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(request),
                    Encoding.UTF8, "application/json")
            };

            if (authorize)
                AddAuthentication(httpRequest);

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                var s = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Service response: " + s);
                var result = await response.Content.ReadAsAsync<T>();
                return result;
            }
        }

        public async Task<T> GetAsAsync<T>(string url,
            CancellationToken cancellationToken = default(CancellationToken),
            bool authorize = false)
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Get,
            };

            if (authorize)
                AddAuthentication(httpRequest);

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                var s = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Service response: " + s);
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(s);
                return result;
            }
        }

        public async Task<string> SendXmlAsStringAsync(string url, string content,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Post,
                Content = new StringContent(content,
                    Encoding.UTF8, "text/html"),
            };

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                var s = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Service response: " + s);
                return s;
            }
        }

        public async Task<byte[]> SendXmlAsByteArrayAsync(string url, string content,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Post,
                Content = new StringContent(content,
                    Encoding.UTF8, "text/html"),
            };

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                var s = await response.Content.ReadAsByteArrayAsync();
                return s;
            }
        }

        public async Task<string> SendAsStringAsync(string url, object request,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(request),
                    Encoding.UTF8, "application/json"),
            };

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                var s = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"Service response: " + s);
                return s;
            }
        }

        public async Task<Stream> SendAsStreamAsync(string url, object request,
            CancellationToken cancellationToken = default(CancellationToken),
            bool authorize = false)
        {
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseUrl}{url}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(request),
                    Encoding.UTF8, "application/json")
            };

            if (authorize)
                AddAuthentication(httpRequest);

            using (var response = await httpClient.SendAsync(httpRequest, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var result = new MemoryStream();
                await stream.CopyToAsync(result);
                result.Position = 0;
                return result;
            }
        }

        private void AddAuthentication(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(AuthToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            }
            //if (!request.Headers.Contains("X-Auth-Token"))
            //    request.Headers.Add("X-Auth-Token", AuthToken);
        }
    }
}
