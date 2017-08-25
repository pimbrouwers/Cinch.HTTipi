using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cinch.HTTipi
{
    public interface ITipi
    {
        Task<T> Get<T>(string url, Dictionary<string, string> headers = null);
        Task<T> Post<T>(string url, string payloadStr, Dictionary<string, string> headers = null);
        Task Post(string url, string payloadStr, Dictionary<string, string> headers = null);
        Task<T> Put<T>(string url, string payloadStr, Dictionary<string, string> headers = null);
        Task Put(string url, string payloadStr, Dictionary<string, string> headers = null);
        Task<T> Delete<T>(string url, Dictionary<string, string> headers = null);
        Task Delete(string url, Dictionary<string, string> headers = null);

        Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder);
        Task Execute(IHTTipiRequestBuilder requestBuilder);
    }

    public class HTTipi : ITipi
    {
        readonly ILogger log;
        readonly HttpClient client;

        public HTTipi(ILoggerFactory logFactory)
        {
            this.log = logFactory.CreateLogger<HTTipi>();
            this.client = new HttpClient();
        }

        public async Task<T> Get<T>(string url, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url).WithHeaders(headers);
            return await Execute<T>(requestBuilder);
        }

        public async Task<T> Post<T>(string url, string payloadStr, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Post)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            return await Execute<T>(requestBuilder);
        }
        public async Task Post(string url, string payloadStr, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Post)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            await Execute(requestBuilder);
        }

        public async Task<T> Put<T>(string url, string payloadStr, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Put)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            return await Execute<T>(requestBuilder);
        }
        public async Task Put(string url, string payloadStr, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Put)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            await Execute(requestBuilder);
        }

        public async Task<T> Delete<T>(string url, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Delete)
                                                            .WithHeaders(headers);

            return await Execute<T>(requestBuilder);
        }
        public async Task Delete(string url, Dictionary<string, string> headers = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Delete)
                                                            .WithHeaders(headers);

            await Execute(requestBuilder);
        }

        public async Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder)
        {
            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
        }
        public async Task Execute(IHTTipiRequestBuilder requestBuilder)
        {
            await ExecuteRequest(requestBuilder.Build());
        }

        async Task<string> ExecuteRequest(HttpRequestMessage req)
        {
            string respStr = string.Empty;

            using (var resp = await client.SendAsync(req))
            using (var strm = await resp.Content.ReadAsStreamAsync())
            {
                bool gzip = false;

                var contentEncoding = resp.Content.Headers.ContentEncoding;
                var contentType = resp.Content.Headers.ContentType;

                if (contentEncoding != null && contentEncoding.Contains("gzip"))
                    gzip = true;

                Stream s = strm;

                if (gzip)
                {
                    s = new GZipStream(strm, CompressionMode.Decompress);
                }

                try
                {
                    using (var sr = new StreamReader(s))
                    {
                        respStr = await sr.ReadToEndAsync();
                        if (!resp.IsSuccessStatusCode)
                        {
                            string err = $"{(int)resp.StatusCode}: {resp.StatusCode} Request({req.RequestUri}) failed with error: {respStr}";
                            log.LogError(err);

                            throw new HTTipiRequestException((int)resp.StatusCode, err);
                        }

                        log.LogInformation($"{(int)resp.StatusCode}: {resp.StatusCode} Request({req.RequestUri}) succeeded");
                    }
                }
                finally
                {
                    s.Dispose();
                }
            }

            return respStr;
        }

        T HandleResponse<T>(string respStr)
        {
            T resp = default(T);

            if (!string.IsNullOrWhiteSpace(respStr))
            {
                if (typeof(T) == typeof(string))
                {
                    resp = (T)(object)respStr;
                }
                else
                {
                    resp = JsonConvert.DeserializeObject<T>(respStr);
                }
            }

            return resp;
        }
    }
}

