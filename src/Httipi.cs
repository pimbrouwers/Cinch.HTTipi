using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Httipi
{
  public static class HttpClientExtensions
  {
    public delegate T ReponseHandler<T>(HttpResponseMessage httpResponseMessage);

    public static async Task Get(
      this HttpClient client,
      string url,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null,
      Action<StreamReader> responseStreamHandler = null)
    {
      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url);
      await client.ExecuteRequest(requestBuilder, responseMessageHandler);
    }

    public static async Task<string> GetJson(
      this HttpClient client,
      string url,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null)
    {
      string resp = string.Empty;

      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url).WithHeaders(headers);
      await client.ExecuteRequest(requestBuilder, responseMessageHandler, async sr => resp = await sr.ReadToEndAsync());

      return resp;
    }

    public static async Task Post(
      this HttpClient client,
      string url,
      HttpContent content,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null,
      Action<StreamReader> responseStreamHandler = null)
    {
      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url)
                                                      .SetMethod(HttpMethod.Post)
                                                      .WithContent(content);

      await client.ExecuteRequest(requestBuilder, responseMessageHandler);
    }

    public static async Task<string> PostJson(
      this HttpClient client,
      string url,
      string json,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null)
    {
      string resp = string.Empty;

      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url)
                                                      .SetMethod(HttpMethod.Post)
                                                      .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

      await client.ExecuteRequest(requestBuilder, responseMessageHandler, async sr => resp = await sr.ReadToEndAsync());

      return resp;
    }

    public static async Task Put(
      this HttpClient client,
      string url,
      HttpContent content,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null,
      Action<StreamReader> responseStreamHandler = null)
    {
      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url)
                                                      .SetMethod(HttpMethod.Put)
                                                      .WithContent(content);

      await client.ExecuteRequest(requestBuilder, responseMessageHandler);
    }

    public static async Task<string> PutJson(
      this HttpClient client,
      string url,
      string json,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null)
    {
      string resp = string.Empty;
      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url)
                                                      .SetMethod(HttpMethod.Put)
                                                      .WithContent(new StringContent(json, Encoding.UTF8, "application/json"));

      await client.ExecuteRequest(requestBuilder, responseMessageHandler, async sr => resp = await sr.ReadToEndAsync());

      return resp;
    }

    public static async Task Delete(
      this HttpClient client,
      string url,
      Dictionary<string, string> headers = null,
      Action<HttpResponseMessage> responseMessageHandler = null,
      Action<StreamReader> responseStreamHandler = null)
    {
      var requestBuilder = new HttpRequestMessageBuilder().SetUrl(url)
                                                      .SetMethod(HttpMethod.Delete);

      await client.ExecuteRequest(requestBuilder, responseMessageHandler);
    }

    public static async Task ExecuteRequest(this HttpClient client, HttpRequestMessageBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null, Action<StreamReader> responseStreamHandler = null)
    {
      await client.ExecuteRequest(requestBuilder.Build(), responseMessageHandler);
    }

    public static async Task ExecuteRequest(this HttpClient client, HttpRequestMessage req, Action<HttpResponseMessage> responseMessageHandler = null, Action<StreamReader> responseStreamHandler = null)
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

              throw new HttipiException((int)resp.StatusCode, err, respStr);
            }

            responseStreamHandler?.Invoke(sr);
            responseMessageHandler?.Invoke(resp);
          }
        }
        finally
        {
          s.Dispose();
        }
      }
    }
  }
}