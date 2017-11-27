using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Swan;
using Unosquare.Net;

namespace WebModule
{
    public class ImageResizerWebModule : WebModuleBase
    {
        public override string Name => nameof(ImageResizerWebModule).Humanize();

        public byte[] ByteImage;

        public ImageResizerWebModule(string folder, double defaultWidth = 500, double defaultHeight = 400)
        {
            if(folder == null)
                throw new ArgumentNullException("path cannot be null");

            AddHandler(ModuleMap.AnyPath, HttpVerbs.Get, (context, ct) => Resize(context, folder, defaultWidth, defaultHeight));
        }

        private async Task<bool> Resize(HttpListenerContext context, string folder, double defaultWidth, double defaultHeight)
        {
            try
            {
                var strings = context.Request.RawUrl.Split('/');
                var image = folder + "/" + strings[1];
                image = image.Replace("/", "\\");

                var imgIn = new Bitmap(image);
                double y = imgIn.Height;
                double x = imgIn.Width;

                if(strings.Length >= 3 && strings.Length < 5)
                {
                    if(strings[2] != "thumb")
                    {
                        if(strings.Length == 3)
                        {
                            defaultWidth = int.Parse(strings[2]);
                            defaultHeight = y * defaultWidth / x;
                        }
                        else if(strings.Length == 4)
                        {
                            defaultWidth = int.Parse(strings[2]);
                            defaultHeight = int.Parse(strings[3]);
                        }
                    }
                }
                else if(strings.Length == 2)
                {
                    defaultWidth = x;
                    defaultHeight = y;
                }
                else
                {
                    throw new NotSupportedException();
                }

                double factorWidth = 1;
                double factorLength = 1;

                if(defaultWidth > 0 && defaultHeight > 0)
                {
                    factorWidth = defaultWidth / x;
                    factorLength = defaultHeight / y;
                }

                var outStream = new MemoryStream();

                var imgOut = new Bitmap((int)(x * factorWidth), (int)(y * factorLength));

                imgOut.SetResolution(72, 72);

                var g = Graphics.FromImage(imgOut);
                g.Clear(Color.White);
                g.DrawImage(imgIn, new Rectangle(0, 0, (int)(factorWidth * x), (int)(factorLength * y)),
                  new Rectangle(0, 0, (int)x, (int)y), GraphicsUnit.Pixel);

                imgOut.Save(outStream, getImageFormat(image));
                ByteImage = outStream.ToArray();

                await context.Response.OutputStream.WriteAsync(ByteImage, 0, ByteImage.Length);
                return true;
            }
            catch
            {
                context.Response.StatusCode = 404;
                return false;
            }
        }

        ImageFormat getImageFormat(String path)
        {
            switch(Path.GetExtension(path))
            {
                case ".bmp":
                return ImageFormat.Bmp;
                case ".gif":
                return ImageFormat.Gif;
                case ".jpg":
                return ImageFormat.Jpeg;
                case ".png":
                return ImageFormat.Png;
                default:
                break;
            }

            return ImageFormat.Jpeg;
        }
    }

}
