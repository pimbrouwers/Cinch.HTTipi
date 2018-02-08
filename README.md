# httipi
A .NET Standard compliant set of extenions methods for `System.Net.HttpClient` that makes making HTTP requests much much simpler. Includes a fluent `HttpRequestMessage` builder.

Compression is automatically enabled if available (from response headers) and streams are used throughout the stack to ensure large requests are handled appropriately.

![NuGet Version](https://img.shields.io/nuget/v/Cinch.HTTipi.svg)
[![Build Status](https://travis-ci.org/pimbrouwers/httipi.svg?branch=master)](https://travis-ci.org/pimbrouwers/httipi)

## Getting Started
A simple example to execute a `GET` request deserializing JSON to CLR object using [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
```csharp
var http = new HttpClient();
var someObject = JsonConvert.DeserializeObject(await http.GetString("http://someurl.com"));
```

A more complex `PATCH` request with a JSON request body and an HMAC authorization header.
```csharp
var http = new HTTipi();
string json = JsonConvert.SerializeObject(new { someProperty = "newPropertyValue" }); 

var req = new HTTipiRequestBuilder().SetUrl("http://someurl.com")
                                    .SetMethod(new HttpMethod("PATCH"))
                                    .WithContent(new StringContent(json, Encoding.UTF8, "application/json"))
                                    .AddHeader("Authorization", "hmac somecrazylonghmackey")

await Execute(req);
```

Exception handling.
```csharp
var http = new HttpClient();

try
{
  var someObject = JsonConvert.DeserializeObject(await http.GetString("http://someurl.com"));
}
catch (HTTipiException ex)
{
  //logging
}
```