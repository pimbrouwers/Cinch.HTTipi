using System;
using System.Collections.Generic;
using System.Text;

namespace Cinch.HTTipi
{
    public class HTTipiRequestException : Exception
    {
        public int StatusCode { get; }
        public override string Message { get; }
        
        public HTTipiRequestException(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
