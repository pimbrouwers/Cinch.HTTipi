using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Cinch.Tipi
{
    public interface ITipiRequestBuilder
    {
        HttpRequestMessage Build();
        ITipiRequestBuilder SetUri(string uri);
        ITipiRequestBuilder SetMethod(HttpMethod method);
        ITipiRequestBuilder WithHeaders(Dictionary<string, string> headers);
        ITipiRequestBuilder WithContent(StringContent content);

        ITipiRequestBuilder AddHeader(string name, string value);
    }

    public class TipiRequestBuilder : ITipiRequestBuilder
    {
        Uri uri;
        HttpMethod method;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        StringContent content;

        public TipiRequestBuilder()
        {
            headers.Add("Accept-Encoding", "gzip, deflate");
        }

        public HttpRequestMessage Build()
        {
            var req = new HttpRequestMessage(method ?? HttpMethod.Get, uri);

            if (headers.Any())
            {
                foreach (var header in headers)
                {
                    req.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (content != null)
            {
                req.Content = content;
            }

            return req;
        }

        public ITipiRequestBuilder AddHeader(string name, string value)
        {
            headers.Add(name, value);
            return this;
        }

        public ITipiRequestBuilder SetMethod(HttpMethod method)
        {
            this.method = method;
            return this;
        }

        public ITipiRequestBuilder SetUri(string uri)
        {
            Uri u;

            if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out u))
            {
                throw new ArgumentException("uri appears to be invalid");
            }

            this.uri = u;
            return this;
        }

        public ITipiRequestBuilder WithContent(StringContent content)
        {
            content = this.content;
            return this;
        }

        public ITipiRequestBuilder WithHeaders(Dictionary<string, string> headers)
        {
            headers.ToList().ForEach(x => this.headers.Add(x.Key, x.Value));
            return this;
        }
    }
}
