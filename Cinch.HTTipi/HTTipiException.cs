using System;
using System.Collections.Generic;
using System.Text;

namespace Cinch.HTTipi
{
    public class HTTipiException : Exception
    {
        public HTTipiException(int statusCode, string message, string body)
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
