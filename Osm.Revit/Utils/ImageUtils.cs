using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Osm.Revit.Utils
{
    class ImageUtils
    {
        static public BitmapSource ConvertBitmapSource(string targetResourceName)
        {
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            Uri resourceUri = new Uri($"pack://application:,,,/{name};component/Resources/" + targetResourceName, UriKind.Absolute);
            return new BitmapImage(resourceUri);
        }

        static public BitmapImage Convert(Bitmap src)
        {
            return Convert(src, ImageFormat.Png);
        }

        static public BitmapImage Convert(Bitmap src, ImageFormat imageFormat)
        {
            MemoryStream ms = new MemoryStream();
            ((Bitmap)src).Save(ms, imageFormat);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        internal static Image Convert(BitmapSource targetBitmapSource, out MemoryStream outStream)
        {
            Bitmap bitmap;
            outStream = new MemoryStream();
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(targetBitmapSource));
            enc.Save(outStream);
            bitmap = new Bitmap(outStream);

            return bitmap;
        }
    }
}
