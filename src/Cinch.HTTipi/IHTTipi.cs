using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cinch.HTTipi
{
  public interface IHTTipi
  {
    Task<T> Get<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task<string> GetString(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task<T> Post<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task Post(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task<T> Put<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task Put(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task<T> Patch<T>(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task Patch(string url, string json, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task<T> Delete<T>(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);
    Task Delete(string url, Dictionary<string, string> headers = null, Action<HttpResponseMessage> responseMessageHandler = null);

    Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null, Action<StreamReader> responseStreamHandler = null);
    Task Execute(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null);
  }
}
