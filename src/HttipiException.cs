﻿using System;

namespace Httipi
{
  public class HttipiException : Exception
  {
    public HttipiException(int statusCode, string message, string body)
    {
      StatusCode = statusCode;
      Message = message;
      Body = body;
    }

    public int StatusCode { get; }
    public override string Message { get; }
    public string Body { get; }
  }
}