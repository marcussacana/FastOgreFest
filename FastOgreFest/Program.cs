using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FastOgreFest {
    class Program {
        static void Main(string[] args) {
            if (!File.Exists("Credentials.ini")) {
                Console.WriteLine("Welcome, I Need catch the hash of your credentials, please, login in the OgreFest Launcher");
                CatchCredentials();
            } else {
                LoginServer.ProxyStatus(false);
                Console.WriteLine("Starting...");
                string[] Credentials = File.ReadAllLines("Credentials.ini");
                const string AuthUrl = "http://api.ogrefest.org/account/launcher/validate_credentials/{0}/{1}";
                const string ConnectUrl = "http://api.ogrefest.org/account/launcher/validate_token/{0}";
                const string OK = "\"error\":false";
                const string TOKEN = "\"token\":\"";
                const string DIR = "GamePath=\"";
                string Response = new WebClient().DownloadString(string.Format(AuthUrl, Credentials[0], Credentials[1]));
                if (!Response.Contains(OK)) {
                    Console.WriteLine("Failed to Auth");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Credentials: OK");
                string Token = GetStringAt(Response.IndexOf(TOKEN) + TOKEN.Length, Response);
                Response = new WebClient().DownloadString(string.Format(ConnectUrl, Token));
                if (!Response.Contains(OK)) {
                    Console.WriteLine("Failed to Auth");
                    Console.ReadKey();
                    return;
                }
                Token = GetStringAt(Response.IndexOf(TOKEN) + TOKEN.Length, Response);
                Console.WriteLine("Token: OK");
                string XML = File.ReadAllText("configuration.xml");
                string GameDir = GetStringAt(XML.IndexOf(DIR) + DIR.Length, XML);
                if (!File.Exists("service.ini")) {
                    Console.WriteLine("Error: Patched service.ini not found");
                    Console.ReadKey();
                    return;
                }
                if (File.Exists(GameDir + "\\service.ini"))
                    File.Delete(GameDir + "\\service.ini");
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + "service.ini", GameDir + "\\service.ini");
                string EXE = GameDir + (Environment.Is64BitOperatingSystem ? "\\bin64\\BlackDesert64.exe" : "\\bin\\BlackDesert32.exe");
                Process.Start(new ProcessStartInfo() {
                    FileName = EXE,
                    Arguments = Token,
                    WorkingDirectory = Path.GetDirectoryName(EXE)
                });
                Console.WriteLine("Game: OK\nStarting...");
                System.Threading.Thread.Sleep(3000);
            }
        }

        private static string GetStringAt(int i, string str) {
            string ret = string.Empty;
            while (str[i] != '"')
                ret += str[i++];
            return ret;
        }

        private static void CatchCredentials() {
            LoginServer s = new LoginServer(80);
            s.Initialize(80);
            LoginServer.ProxyStatus(true);
            s.RunServer();
        }
    }
}
