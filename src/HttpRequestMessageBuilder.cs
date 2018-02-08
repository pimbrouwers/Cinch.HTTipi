using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Cinch.Httipi
{
  public class HttpRequestMessageBuilder : IHttpRequestMessageBuilder
  {
    Uri uri;
    HttpMethod method;
    Dictionary<string, string> headers = new Dictionary<string, string>();
    HttpContent content;

    public HttpRequestMessageBuilder()
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

    public IHttpRequestMessageBuilder AddHeader(string name, string value)
    {
      headers.Add(name, value);
      return this;
    }

    public IHttpRequestMessageBuilder SetMethod(HttpMethod method)
    {
      this.method = method;
      return this;
    }

    public IHttpRequestMessageBuilder SetUrl(string uri)
    {
      Uri u;

      if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out u))
      {
        throw new ArgumentException("uri appears to be invalid");
      }

      this.uri = u;
      return this;
    }

    public IHttpRequestMessageBuilder WithContent(HttpContent content)
    {
      this.content = content;
      return this;
    }

    public IHttpRequestMessageBuilder WithHeaders(Dictionary<string, string> headers)
    {
      headers?.ToList().ForEach(x => this.headers.Add(x.Key, x.Value));
      return this;
    }
  }
}
