using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Httipi
{
  public class HttpRequestMessageBuilder
  {
    private Uri uri;
    private HttpMethod method;
    private Dictionary<string, string> headers = new Dictionary<string, string>();
    private HttpContent content;

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

    public HttpRequestMessageBuilder AddHeader(string name, string value)
    {
      headers.Add(name, value);
      return this;
    }

    public HttpRequestMessageBuilder SetMethod(HttpMethod method)
    {
      this.method = method;
      return this;
    }

    public HttpRequestMessageBuilder SetUrl(string uri)
    {
      Uri u;

      if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out u))
      {
        throw new ArgumentException("uri appears to be invalid");
      }

      this.uri = u;
      return this;
    }

    public HttpRequestMessageBuilder WithContent(HttpContent content)
    {
      this.content = content;
      return this;
    }

    public HttpRequestMessageBuilder WithHeaders(Dictionary<string, string> headers)
    {
      headers?.ToList().ForEach(x => this.headers.Add(x.Key, x.Value));
      return this;
    }
  }
}