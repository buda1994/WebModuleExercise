﻿using System;
using System.IO;
using System.Reflection;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;

namespace WebModule
{
    class Program
    {
        public static string HtmlRootPath
        {
            get
            {
                var assemblyPath = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);

                return Path.Combine(Directory.GetParent(assemblyPath).Parent.Parent.FullName, "Warsong.png");
            }
        }

        static void Main(string[] args)
        {
            var url = "http://localhost:9696/";

            using(var server = new WebServer(url, RoutingStrategy.Regex))
            {
                server.RegisterModule(new ImageResizerWebModule(HtmlRootPath,0,0));
                
                server.RunAsync();

                var browser = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo(url + "api/echo/Warsong") { UseShellExecute = true }
                };
                browser.Start();

                Console.ReadKey(true);
            }
        }
    }
}