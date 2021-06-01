// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using Cosmos.System.Network.IPv4.TCP;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace SimpleHttpServer
{

    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener Listener;
        private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        #region Public Methods
        public HttpServer(int port, List<Route> routes)
        {
            this.Port = port;
            this.Processor = new HttpProcessor();

            foreach (var route in routes)
            {
                this.Processor.AddRoute(route);
            }
        }

        public void Listen()
        {
            Listener = new TcpListener(Port);
            Listener.Start();
            TcpClient s = Listener.AcceptTcpClient();
            Listener.Stop();

            while (IsActive)
            {
                Processor.HandleClient(s);
            }
        }

        #endregion

    }
}



