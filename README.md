# Cinch.HTTipi
A thin wraper around System.Net.HttpClient that makes making HTTP requests much much simpler, effectively turning several dozen lines of code into one. 

Compression is automatically enabled if available (from response headers) and streams are used throughout the stack to ensure large requests are handled appropriately.

![NuGet Version](https://img.shields.io/nuget/v/Cinch.HTTipi.svg)
[![Build Status](https://travis-ci.org/pimbrouwers/httipi.svg?branch=master)](https://travis-ci.org/pimbrouwers/httipi)

## Getting Started
A simple example to execute a `GET` request deserializing JSON to CLR object.
```csharp
try
{
  var someObject = await http.Get<SomeObject>("http://someurl.com");
}
catch (HTTipiException ex)
{
  //logging
}
```

A more complex `PATCH` request with custom `HttpContent`.
```csharp
string json = JsonConvert.SerializeObject(patchOperation);

var headers = new Dictionary<string, string>();
headers.Add("Authorization", "hmac somecrazylonghmackey");

var req = new HTTipiRequestBuilder().SetUrl("http://someurl.com")
                                    .SetMethod(new HttpMethod("PATCH"))
                                    .WithContent(new StringContent(json, Encoding.UTF8, "application/json"))
                                    .AddHeader("Authorization", "hmac somecrazylonghmackey")

var someObject = await Execute<SomeObject>(req);
```

## API

### Base

```csharp
Task<T> Execute<T>(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null, Action<StreamReader> responseStreamHandler = null);
Task Execute(IHTTipiRequestBuilder requestBuilder, Action<HttpResponseMessage> responseMessageHandler = null);
```

### Helper Methods

```csharp
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
```

### Request Builder

```csharp
HttpRequestMessage Build();
IHTTipiRequestBuilder SetUrl(string uri);
IHTTipiRequestBuilder SetMethod(HttpMethod method);
IHTTipiRequestBuilder WithHeaders(Dictionary<string, string> headers);
IHTTipiRequestBuilder WithContent(HttpContent content);
IHTTipiRequestBuilder AddHeader(string name, string value);
````

