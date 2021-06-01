using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError()
        {
            string content = @"<h1>Internal Server Error</h1>
<small>by SimpleHtppServer</small>";

            return new HttpResponse()
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
                Content = Encoding.ASCII.GetBytes(content)
            };
        }

        public static HttpResponse NotFound()
        {
            string content = @"<h1>Not Found</h1>
<small>by SimpleHtppServer</small>";

            return new HttpResponse()
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                Content = Encoding.ASCII.GetBytes(content)
            };
        }
    }
}
