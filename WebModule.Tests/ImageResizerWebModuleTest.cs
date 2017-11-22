using System;
using NUnit.Framework;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using System.IO;
using System.Reflection;
using System.Net;

namespace WebModule.Tests
{
    [TestFixture]
    class ImageResizerWebModuleTest
    {
        public static string HtmlRootPath
        {
            get
            {
                var assemblyPath = Path.GetDirectoryName(typeof(ImageResizerWebModuleTest).GetTypeInfo().Assembly.Location);

                return Path.Combine(Directory.GetParent(assemblyPath).Parent.Parent.FullName, "images");
            }
        }

        [TestCase("Warsong.png", "http://localhost:9696/Warsong.png")]
        [TestCase("Warsong.png/700", "http://localhost:9696/Warsong.png/700")]
        [TestCase("Warsong.png/700/400", "http://localhost:9696/Warsong.png/700/400")]
        public void WithValidPath_DoesNotThrowException(string urlParams, string expected)
        {
            var url = "http://localhost:9696/";

            using(var server = new WebServer(url, RoutingStrategy.Regex))
            {
                server.RegisterModule(new ImageResizerWebModule(HtmlRootPath));

                server.RunAsync();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + urlParams);

                Uri siteUri = new Uri(expected);

                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Assert.AreEqual(siteUri, response.ResponseUri);
                    Assert.AreEqual("OK",response.StatusDescription);
                }

                Assert.AreEqual(server.UrlPrefixes[0], "http://localhost:9696/");
            }
        }

        [Test]
        public void WithNullPath_ThrowsArgumentNullException()
        {
            var url = "http://localhost:9696/";

            using(var server = new WebServer(url, RoutingStrategy.Regex))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    server.RegisterModule(new ImageResizerWebModule(null)));
            }
        }

        [Test]
        public void WithInvalidPath_ThrowsWebException()
        {
            var url = "http://localhost:9696/";

            using(var server = new WebServer(url, RoutingStrategy.Regex))
            {
                server.RegisterModule(new ImageResizerWebModule("Invalid/Path"));

                server.RunAsync();

                HttpWebResponse response;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                
                Assert.Throws<WebException>(() =>
                    response = (HttpWebResponse)request.GetResponse());
            }
        }
    }
}
