using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Cinch.HTTipi
{
    public interface IHTTipiRequestBuilder
    {
        HttpRequestMessage Build();
        IHTTipiRequestBuilder SetUrl(string uri);
        IHTTipiRequestBuilder SetMethod(HttpMethod method);
        IHTTipiRequestBuilder WithHeaders(Dictionary<string, string> headers);
        IHTTipiRequestBuilder WithContent(HttpContent content);

        IHTTipiRequestBuilder AddHeader(string name, string value);
    }

    public class HTTipiRequestBuilder : IHTTipiRequestBuilder
    {
        Uri uri;
        HttpMethod method;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        HttpContent content;

        public HTTipiRequestBuilder()
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

        public IHTTipiRequestBuilder AddHeader(string name, string value)
        {
            headers.Add(name, value);
            return this;
        }

        public IHTTipiRequestBuilder SetMethod(HttpMethod method)
        {
            this.method = method;
            return this;
        }

        public IHTTipiRequestBuilder SetUrl(string uri)
        {
            Uri u;

            if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out u))
            {
                throw new ArgumentException("uri appears to be invalid");
            }

            this.uri = u;
            return this;
        }

        public IHTTipiRequestBuilder WithContent(HttpContent content)
        {
            this.content = content;
            return this;
        }

        public IHTTipiRequestBuilder WithHeaders(Dictionary<string, string> headers)
        {
            headers?.ToList().ForEach(x => this.headers.Add(x.Key, x.Value));
            return this;
        }
    }
}
