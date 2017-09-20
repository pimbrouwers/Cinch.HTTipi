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
    public interface IHTTipi
    {
        Task<T> Get<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task<T> Post<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task Post(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task<T> Put<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task Put(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task<T> Patch<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task Patch(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task<T> Delete<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
        Task Delete(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);

        Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null);
        Task Execute(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null);
    }

    public class HTTipi : IHTTipi
    {
        readonly ILogger<HTTipi> log;
        readonly HttpClient client;
        readonly JsonSerializerSettings jsonSettings;

        public HTTipi(ILogger<HTTipi> logger, JsonSerializerSettings jsonSettings = null)
        {
            this.log = logger;
            this.client = new HttpClient();

            if (jsonSettings != null)
            {
                this.jsonSettings = jsonSettings;
            }

        }

        public async Task<T> Get<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url).WithHeaders(headers);
            return await Execute<T>(requestBuilder, responseMessageHandler);
        }

        public async Task<T> Post<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Post)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

            return await Execute<T>(requestBuilder, responseMessageHandler);
        }
        public async Task Post(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Post)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

            await Execute(requestBuilder, responseMessageHandler);
        }

        public async Task<T> Put<T>(string url, string json = null, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Put)
                                                            .WithHeaders(headers);

            if (json != null)
            {
                requestBuilder.WithContent(new StringContent(json, Encoding.UTF8, "application/json"));
            }


            return await Execute<T>(requestBuilder, responseMessageHandler);
        }
        public async Task Put(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Put)
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

            await Execute(requestBuilder, responseMessageHandler);
        }

        public async Task<T> Patch<T>(string url, string json = null, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(new HttpMethod("PATCH"))
                                                            .WithHeaders(headers);

            if (json != null)
            {
                requestBuilder.WithContent(new StringContent(json, Encoding.UTF8, "application/json"));
            }


            return await Execute<T>(requestBuilder, responseMessageHandler);
        }
        public async Task Patch(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(new HttpMethod("PATCH"))
                                                            .WithHeaders(headers)
                                                            .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

            await Execute(requestBuilder, responseMessageHandler);
        }

        public async Task<T> Delete<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Delete)
                                                            .WithHeaders(headers);

            return await Execute<T>(requestBuilder, responseMessageHandler);
        }
        public async Task Delete(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            var requestBuilder = new HTTipiRequestBuilder().SetUrl(url)
                                                            .SetMethod(HttpMethod.Delete)
                                                            .WithHeaders(headers);

            await Execute(requestBuilder, responseMessageHandler);
        }

        public async Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            T resp = default(T);

            await ExecuteRequest(requestBuilder.Build(), responseMessageHandler, sr =>
            {                
                resp = HandleResponse<T>(sr);
            });

            return resp;
        }
        public async Task Execute(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null)
        {
            await ExecuteRequest(requestBuilder.Build(), responseMessageHandler);
        }

        async Task ExecuteRequest(HttpRequestMessage req, Action<HttpResponseMessage> responseMessageHandler = null, Action<StreamReader> internalResponseStreamHandler = null)
        {
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
                        if (!resp.IsSuccessStatusCode)
                        {
                            string respStr = await sr.ReadToEndAsync();
                            string err = $"{(int)resp.StatusCode}: {resp.StatusCode} Request({req.RequestUri}) failed with error: {respStr}";
                            log.LogError(err);

                            throw new HTTipiException((int)resp.StatusCode, err, respStr);
                        }

                        log.LogInformation($"{(int)resp.StatusCode}: {resp.StatusCode} Request({req.RequestUri}) succeeded");
                        
                        internalResponseStreamHandler?.Invoke(sr);

                        responseMessageHandler?.Invoke(resp);
                    }
                }
                finally
                {
                    s.Dispose();
                }
            }
        }

        T HandleResponse<T>(StreamReader sr)
        {
            log.LogInformation($"Running internalResponseStreamHandler()");
            using (var jsonReader = new JsonTextReader(sr))
            {
                JsonSerializer jsonSerializer;

                if (jsonSettings != null)
                {
                    jsonSerializer = JsonSerializer.Create(jsonSettings);
                }
                else
                {
                    jsonSerializer = new JsonSerializer();
                }

                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }
    }
}

