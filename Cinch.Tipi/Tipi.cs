using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cinch.Tipi
{
    public interface ITipi
    {        
        Task<T> Get<T>(string url);
        Task<T> Post<T>(string url, string payloadStr);
        Task<T> Put<T>(string url, string payloadStr);
        Task<T> Delete<T>(string url);

        Task<T> Execute<T>(ITipiRequestBuilder requestBuilder);
    }

    public class Tipi : ITipi
    {
        readonly ILogger log;
        readonly HttpClient client;

        public Tipi(ILoggerFactory logFactory)
        {
            this.log = logFactory.CreateLogger<Tipi>();
            this.client = new HttpClient();
        }

        public async Task<T> Get<T>(string url)
        {
            var requestBuilder = new TipiRequestBuilder().SetUri(url);
            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
        }

        public async Task<T> Post<T>(string url, string payloadStr)
        {
            var requestBuilder = new TipiRequestBuilder().SetUri(url)
                                              .SetMethod(HttpMethod.Post)
                                              .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
        }

        public async Task<T> Put<T>(string url, string payloadStr)
        {
            var requestBuilder = new TipiRequestBuilder().SetUri(url)
                                              .SetMethod(HttpMethod.Put)
                                              .WithContent(new StringContent(payloadStr, Encoding.UTF8, "application/json"));

            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
        }

        public async Task<T> Delete<T>(string url)
        {
            var requestBuilder = new TipiRequestBuilder().SetUri(url)
                                              .SetMethod(HttpMethod.Delete);

            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
        }
        
        public async Task<T> Execute<T>(ITipiRequestBuilder requestBuilder)
        {
            var respStr = await ExecuteRequest(requestBuilder.Build());

            return HandleResponse<T>(respStr);
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

                            throw new HttpRequestException(err);
                        }
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
