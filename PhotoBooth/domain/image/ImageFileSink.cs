#region PhotoBooth - MIT - (c) 2015 Patrick Bronneberg
/*
  PhotoBooth - an application to control a DIY photobooth

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
  
  Copyright 2015 Patrick Bronneberg
*/
#endregion

using com.prodg.photobooth.common;
using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.IO;
using com.prodg.photobooth.domain.image;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// Sink to store an image to disk in an image file
    /// </summary>
    public class ImageFileSink : ISingleImageProcessor, IDisposable
    {
        private readonly ILogger logger;
        private EncoderParameters imageEncoderParameters;
        private readonly ImageCodecInfo imageCodecInfo;

        private readonly string fileName;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fileName"></param>
        /// <param name="quality"></param>
        public ImageFileSink(ILogger logger, string fileName, int quality)
        {
            this.logger = logger;
            this.fileName = fileName;
            logger.LogDebug("Creating ImageFileSink for: "+fileName);
          
            // EncoderParameter object in the array.
            imageEncoderParameters = new EncoderParameters(1);
            imageEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            imageCodecInfo = GetEncoderInfo("image/jpeg");
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession session, Image image)
        {
            logger.LogInfo(string.Format("Storing file to disk {0}: {1}", session.StoragePath, fileName));
                       if (image == null)
            {
                throw new ArgumentNullException("image", "image may not be null");
            }

            image.Save(Path.Combine(session.StoragePath, fileName), imageCodecInfo, imageEncoderParameters);
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

        #region IDisposable Implementation

		bool disposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
		    if (!disposed)
		    {
		        if (disposing)
		        {
		            // Clean up managed objects
		           
                    if (imageEncoderParameters != null)
                    {
                        imageEncoderParameters.Dispose();
                        imageEncoderParameters = null;
                    }
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

        ~ImageFileSink()
		{
			Dispose (false);
		}
		
		#endregion

    }
}
