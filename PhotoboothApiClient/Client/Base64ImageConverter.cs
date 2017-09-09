using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ItsMyParty.Photobooth.Client
{
    public static class Base64ImageConverter
    {
        public static byte[] FromBase64(string base64EncodedImage)
        {
            var match = Regex.Match(base64EncodedImage, "^data:(?<format>[^;]*?);base64,(?<body>.*?)$");
            if (match.Groups.Count.Equals(3))
            {
                var format = match.Groups["format"].Value;
                Console.WriteLine("imageformat = " + format);

                var body = match.Groups["body"].Value;
                return Convert.FromBase64String(body);
            }
            return null;
        }

        /// <summary>
        /// Returns a Base64 encoded string from the given image JPG buffer
        /// </summary>
        /// <example><see href="data:image/gif;base64,R0lGODlhAQABAIABAEdJRgAAACwAAAAAAQABAAACAkQBAA=="/></example>
        /// <param name="buffer"></param>
        public static string ToBase64Jpg(byte[] buffer)
        {
            var imageFormat = "image/jpeg";
            return ToBase64(buffer, imageFormat);
        }


        /// <summary>
        /// Returns a Base64 encoded string from the given image PNG buffer
        /// </summary>
        /// <example><see href="data:image/gif;base64,R0lGODlhAQABAIABAEdJRgAAACwAAAAAAQABAAACAkQBAA=="/></example>
        /// <param name="buffer"></param>
        public static string ToBase64Png(byte[] buffer)
        {
            var imageFormat = "image/png";
            return ToBase64(buffer, imageFormat);
        }

        public static string ToBase64(byte[] buffer, string imageFormat)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return $"data:{imageFormat};base64,{Convert.ToBase64String(stream.ToArray())}";
            }
        }
    }
}
