using Microsoft.Extensions.Logging;
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
        Task<string> Get(string url);
        Task<T> Get<T>(string url);

        Task<string> Post(string url, string payloadStr);
        Task<string> Put(string url, string payloadStr);
        Task<string> Delete(string url);
    }

    public class Tipi : ITipi
    {
        ILogger log;
        HttpClient client;

        public Tipi(ILoggerFactory logFactory)
        {
            this.log = logFactory.CreateLogger<Tipi>();
            this.client = new HttpClient();
        }

        public async Task<string> Delete(string url)
        {
            Uri uri = ParseUrl(url);

            InitializeClient(uri);

            return await ExecuteRequest(BuildRequest(HttpMethod.Delete, uri));
        }

        public async Task<string> Get(string url)
        {
            Uri uri = ParseUrl(url);

            InitializeClient(uri);

            return await ExecuteRequest(BuildRequest(HttpMethod.Get, uri));
        }

        public async Task<T> Get<T>(string url)
        {
            T resp = default(T);
            var respStr = await Get(url);

            if (!string.IsNullOrWhiteSpace(respStr))
            {
                resp = JsonConvert.DeserializeObject<T>(respStr);
            }

            return resp;
        }

        public async Task<string> Post(string url, string payloadStr)
        {
            Uri uri = ParseUrl(url);

            InitializeClient(uri);

            return await ExecuteRequest(BuildRequest(HttpMethod.Post, uri, payloadStr));
        }

        public async Task<string> Put(string url, string payloadStr)
        {
            Uri uri = ParseUrl(url);

            InitializeClient(uri);

            return await ExecuteRequest(BuildRequest(HttpMethod.Put, uri, payloadStr));
        }

        HttpRequestMessage BuildRequest(HttpMethod method, Uri uri, string payloadStr = null)
        {
            var req = new HttpRequestMessage(method, uri.PathAndQuery);

            if (!string.IsNullOrWhiteSpace(payloadStr))
                req.Content = new StringContent(payloadStr, Encoding.UTF8, "application/json");

            return req;
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

        void InitializeClient(Uri uri)
        {
            client.BaseAddress = new Uri($"{uri.Scheme}://{uri.Authority}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        }

        Uri ParseUrl(string url)
        {
            Uri uri = null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException("The url parameter is not a valid absolute url");
            }

            return uri;
        }

    }
}
