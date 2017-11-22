using System;
using NUnit.Framework;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using System.IO;
using System.Reflection;
using System.Net;
using System.Drawing;

namespace WebModule.Tests
{
    [TestFixture]
    class ImageResizerWebModuleTest
    {
        string Url = "http://localhost:9696/";

        public static string HtmlRootPath
        {
            get
            {
                var assemblyPath = Path.GetDirectoryName(typeof(ImageResizerWebModuleTest).GetTypeInfo().Assembly.Location);

                return Path.Combine(Directory.GetParent(assemblyPath).Parent.Parent.FullName, "images");
            }
        }

        [TestCase("Warsong.png", 395, 395)]
        [TestCase("Warsong.png/700", 700, 700)]
        [TestCase("Warsong.png/thumb", 500, 400)]
        [TestCase("Warsong.png/700/500", 700, 500)]
        public void WithValidPath_ReturnsImage(string urlParams, int expectedWidth, int expectedHeight)
        {
            using(var server = new WebServer(Url, RoutingStrategy.Regex))
            {
                server.RegisterModule(new ImageResizerWebModule(HtmlRootPath));

                server.RunAsync();

                using(var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(Url + urlParams);

                    using(var ms = new MemoryStream(imageBytes))
                    {
                        var image = Image.FromStream(ms);
                        Assert.AreEqual(image.Height, expectedHeight);
                        Assert.AreEqual(image.Width, expectedWidth);
                    }
                }
            }
        }

        [Test]
        public void WithNullPath_ThrowsArgumentNullException()
        {
            using(var server = new WebServer(Url, RoutingStrategy.Regex))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    server.RegisterModule(new ImageResizerWebModule(null)));
            }
        }

        [Test]
        public void WithInvalidPath_ThrowsWebException()
        {
            using(var server = new WebServer(Url, RoutingStrategy.Regex))
            {
                server.RegisterModule(new ImageResizerWebModule("Invalid/Path"));

                server.RunAsync();

                HttpWebResponse response;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                
                Assert.Throws<WebException>(() =>
                    response = (HttpWebResponse)request.GetResponse());
            }
        }
    }
}
