// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using Cosmos.HAL;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleHttpServer
{
    public class HttpProcessor
    {

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        #endregion

        #region Constructors

        public HttpProcessor()
        {
        }

        #endregion

        #region Public Methods
        public void HandleClient(TcpClient tcpClient)
        {
            HttpRequest request = GetRequest(tcpClient);

            // route and handle the request...
            HttpResponse response = RouteRequest(tcpClient, request);      
          
            Console.WriteLine("{0} {1}",response.StatusCode,request.Url);
            // build a default response for errors
            if (response.Content == null) {
                if (response.StatusCode != "200") {
                    response.Content = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                }
            }

            WriteResponse(tcpClient, response);

            tcpClient.Close();
        }

        // this formats the HTTP response...
        private static void WriteResponse(TcpClient client, HttpResponse response) {            
            if (response.Content == null) {
                response.Content = "";
            }
            
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type")) {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            var sb = new StringBuilder();
            sb.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));

            foreach (var str in response.Headers)
            {
                sb.Append(str.Key + ": " + str.Value + "\r\n");
            }

            sb.Append("\r\n");
            sb.Append(response.Content);

            client.Send(Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public void AddRoute(Route route)
        {
            this.Routes.Add(route);
        }

        #endregion

        #region Private Methods

        protected virtual HttpResponse RouteRequest(TcpClient client, HttpRequest request)
        {
            List<Route> routes = this.Routes;

            if (routes.Count == 0)
                return HttpBuilder.NotFound();

            Route route = null;

            foreach (var x in routes)
            {
                if (x.Method == request.Method)
                {
                    route = x;
                    break;
                }
            }

            if (route == null)
                return new HttpResponse()
                {
                    ReasonPhrase = "Method Not Allowed",
                    StatusCode = "405",

                };

            // extract the path if there is one
            request.Path = request.Url;

            // trigger the route handler...
            request.Route = route;
            try {
                return new HttpResponse()
                {
                    Content = route.Content,
                    ReasonPhrase = "OK",
                    StatusCode = "200"
                };
            } catch(Exception ex) {
                Global.mDebugger.Send(ex.Message);
                return HttpBuilder.InternalServerError();
            }

        }

        private HttpRequest GetRequest(TcpClient client)
        {
            //Read Request Line
            var ep = new EndPoint(Address.Zero, 0);
            string request = Encoding.ASCII.GetString(client.Receive(ref ep));
            string[] xrequest = request.Split(new[] { "\r\n" }, StringSplitOptions.None);

            string[] tokens = xrequest[0].Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            var headers = new Dictionary<string, string>();
            string line;
            int counter = 1;
            while ((line = xrequest[counter]) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
                counter++;
            }

            var content = new StringBuilder();
            if (headers.ContainsKey("Content-Length"))
            {
                while (counter < xrequest.Length)
                {
                    content.AppendLine(xrequest[counter]);
                    counter++;
                }
            }

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content.ToString()
            };
        }

        #endregion


    }
}
