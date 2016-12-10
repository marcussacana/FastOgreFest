namespace FastOgreFest {
    using System;
    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class HttpProcessor
    {
        private const int BufferSize = 0x1000;
        public string Method;
        public string ProtocolVersionstring;
        public string HttpUrl;
        public Hashtable Headers = new Hashtable();
        private Stream InputStream;
        private static int MaxPortSize = 0xa00000;
        public Stream OutputStream;
        public TcpClient Socket;
        public HttpServer Server;

        public HttpProcessor(TcpClient Socket, HttpServer Server)
        {
            this.Socket = Socket;
            this.Server = Server;
        }

        public void handleGETRequest()
        {
            Server.handleGETRequest(this);
        }

        public void handlePOSTRequest()
        {
            int num = 0;
            MemoryStream stream = new MemoryStream();
            if (Headers.ContainsKey("Content-Length"))
            {
                num = Convert.ToInt32(Headers["Content-Length"]);
                if (num > MaxPortSize)
                {
                    throw new Exception($"POST Content-Length({num}) too big for this simple server");
                }
                byte[] buffer = new byte[0x1000];
                int num2 = num;
                while (num2 > 0)
                {
                    int count = InputStream.Read(buffer, 0, Math.Min(0x1000, num2));
                    if (count == 0)
                    {
                        if (num2 != 0)
                        {
                            throw new Exception("client disconnected during post");
                        }
                        break;
                    }
                    num2 -= count;
                    stream.Write(buffer, 0, count);
                }
                stream.Seek(0L, SeekOrigin.Begin);
            }
            Server.handlePOSTRequest(this, new StreamReader(stream));
        }

        public void ParseRequest()
        {
            char[] separator = new char[] { ' ' };
            string[] strArray = streamReadLine(InputStream).Split(separator);
            if (strArray.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            Method = strArray[0].ToUpper();
            HttpUrl = strArray[1];
            ProtocolVersionstring = strArray[2];
        }

        public void Process()
        {
            InputStream = new BufferedStream(Socket.GetStream());
            OutputStream = new BufferedStream(Socket.GetStream());
            try
            {
                ParseRequest();
                ReadHeaders();
                if (Method.Equals("GET"))
                {
                    handleGETRequest();
                }
                else if (Method.Equals("POST"))
                {
                    handlePOSTRequest();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception: " + exception.ToString());
                Failure();
            }
            TryClose(InputStream);
            TryClose(OutputStream);
            Socket.Close();
        }

        private void TryClose(Stream stream) {
            try {
                stream.Close();
            }
            catch { }
        }
        public void ReadHeaders()
        {
            for (string Header = streamReadLine(InputStream); !string.IsNullOrEmpty(Header); Header = streamReadLine(InputStream))
            {
                int index = Header.IndexOf(':');
                if (index == -1)
                {
                    throw new Exception("invalid http header line: " + Header);
                }
                string Name = Header.Substring(0, index);
                int startIndex = index + 1;
                while ((startIndex < Header.Length) && (Header[startIndex] == ' '))
                {
                    startIndex++;
                }
                string Value = Header.Substring(startIndex, Header.Length - startIndex);
                Headers[Name] = Value;
            }
        }
        
        private string streamReadLine(Stream inputStream)
        {
            string str = "";
            while (true)
            {
                int num = inputStream.ReadByte();
                if (num == 10)
                {
                    return str;
                }
                if (num != 13)
                {
                    if (num == -1)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        str = str + Convert.ToChar(num).ToString();
                    }
                }
            }
        }

        public void Failure()
        {
            WriteLine("HTTP/1.0 404 File not found");
            WriteLine("Connection: close");
            WriteLine("");
        }

        public void WriteLine(string Line) {
            byte[] Buffer = System.Text.Encoding.UTF8.GetBytes(Line + "\r\n");
            OutputStream.Write(Buffer, 0, Buffer.Length);
        }
        public void Success(string content_type = "text/html", long length = -1) {
            WriteLine("HTTP/1.1 200 OK");
            WriteLine("Server: Apache/2.4.18 (Ubuntu)");
            WriteLine("Content-Type: " + content_type);
            if (length != -1)
                WriteLine("Content-Length: " + length);
            WriteLine("");
        }
    }
}

