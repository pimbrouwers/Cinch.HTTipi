using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Cinch.Httipi
{
  public interface IHttpRequestMessageBuilder
  {
    HttpRequestMessage Build();
    IHttpRequestMessageBuilder SetUrl(string uri);
    IHttpRequestMessageBuilder SetMethod(HttpMethod method);
    IHttpRequestMessageBuilder WithHeaders(Dictionary<string, string> headers);
    IHttpRequestMessageBuilder WithContent(HttpContent content);
    IHttpRequestMessageBuilder AddHeader(string name, string value);
  }
}
