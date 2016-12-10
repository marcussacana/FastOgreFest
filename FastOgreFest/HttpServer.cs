namespace FastOgreFest {
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public abstract class HttpServer
    {
        private bool is_active = true;
        private TcpListener listener;
        protected int port;

        public HttpServer(int port)
        {
            this.port = port;
        }
        private HttpProcessor Processor;
        public void Sucess(string ContentType = "text/html") => Processor.Success(ContentType);
        public void Failure() => Processor.Failure();
        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
        public void RunServer()
        {
            IPAddress localaddr = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(localaddr, port);
            listener.Start();
            while (is_active)
            {
                Processor = new HttpProcessor(listener.AcceptTcpClient(), this);
                new Thread(new ThreadStart(Processor.Process)).Start();
                Thread.Sleep(1);
            }
        }
    }
}

