namespace FastOgreFest {
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class LoginServer : HttpServer
    {
        public void Initialize(int Port) {
        }        

        public static string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");

        public bool Working = false;
        private static string Redirector = "127.0.0.1 api.ogrefest.org";
        public static void ProxyStatus(bool value) {
            string[] Lines = File.ReadAllLines(HostsPath, System.Text.Encoding.UTF8);
            int Index = -1;
            for (int i = 0; i < Lines.Length; i++)
                if (Lines[i].EndsWith(Redirector)) {
                    Index = i;
                    break;
                }
            if (Index == -1) {
                Index = Lines.Length;
                Array.Resize(ref Lines, Lines.Length + 1);
            }
            Lines[Index] = value ? Redirector : "#" + Redirector;
            File.WriteAllLines(HostsPath, Lines);
        }
        
        public LoginServer(int port) : base(port) { }

        public override void handleGETRequest(HttpProcessor Connection)
        {
            const string MASK = "/account/launcher/validate_credentials/";
            string URL = Connection.HttpUrl;
            if (URL.StartsWith(MASK)) {
                string SUB = URL.Substring(MASK.Length, URL.Length - MASK.Length);
                string[] Credentials = SUB.Split('/');
                File.WriteAllLines("Credentials.ini", Credentials);
                Console.WriteLine("Credentials: OK");
                ProxyStatus(false);
                Environment.Exit(0);
            }
            while (Working)
                System.Threading.Thread.Sleep(new Random().Next(1, 1000));
            Working = true;
            ProxyStatus(false);
            string resp = new WebClient().DownloadString("http://api.ogrefest.org" + URL);
            Connection.Success("application/json", resp.Length);
            byte[] Buffer = Encoding.UTF8.GetBytes(resp);
            Connection.OutputStream.Write(Buffer, 0, Buffer.Length);
            Connection.OutputStream.Close();
            ProxyStatus(true);
            Working = false;
        }
        public override void handlePOSTRequest(HttpProcessor Connection, StreamReader inputData)
        {
            Connection.Failure();
        }
    }
}

