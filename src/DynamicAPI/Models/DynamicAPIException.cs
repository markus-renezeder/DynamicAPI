using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DynamicAPI.Models
{
    public class DynamicAPIException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public DynamicAPIException()
        { 
        
        }

        public DynamicAPIException(HttpStatusCode statusCode, string? message) : base(message) 
        {
            this.StatusCode = statusCode;
        }

        public DynamicAPIException(HttpStatusCode statusCode, string? message, Exception? innerException) : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

    }
}
