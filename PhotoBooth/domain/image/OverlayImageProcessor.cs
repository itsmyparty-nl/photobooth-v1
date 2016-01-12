#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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
  
  Copyright 2014 Patrick Bronneberg
*/
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes an image by applying a filter
    /// </summary>
    public class OverlayImageProcessor : ISingleImageProcessor, IDisposable
    {
        private readonly ILogger logger;
        private ImageAttributes attributes;
        private readonly string overlayImageFileName;
        private Image overlayImage;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="overlayImageFileName"></param>
        public OverlayImageProcessor(ILogger logger, string overlayImageFileName)
        {
            this.logger = logger;
            this.overlayImageFileName = overlayImageFileName;

            logger.LogDebug("Creating OverlayImageProcessor: " + overlayImageFileName);
            if (!File.Exists(overlayImageFileName))
            {
                throw new FileNotFoundException("overlay image does not exist: " + overlayImageFileName);
            }

            overlayImage = Image.FromFile(overlayImageFileName);

            //create some image attributes
            attributes = new ImageAttributes();
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession session, Image image)
        {            
            logger.LogInfo("Applying overlay on image: " + Path.GetFileNameWithoutExtension(overlayImageFileName));

            if (image == null)
            {
                throw new ArgumentNullException("image", "image may not be null");
            }

            //get a graphics object from the image so we can draw on it
            using (var graphics = Graphics.FromImage(image))
            {
                var srcRectangle = new Rectangle(0, 0, image.Width, image.Height);
                int xStart = (int) Math.Round((image.Width/2f) - (overlayImage.Width/2f));
                int yStart = (int) Math.Round((image.Height/2f) - (overlayImage.Height/2f));
                //int xStart = (int) Math.Round(image.Width/2f);
                //int yStart = (int) Math.Round(image.Height/2f);
                
                //graphics.DrawImage(overlayImage, srcRectangle, xStart, yStart, overlayImage.Width, overlayImage.Height,
                //    GraphicsUnit.Pixel, attributes);
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(overlayImage, xStart, yStart,
                    new Rectangle(0, 0, overlayImage.Width, overlayImage.Height), GraphicsUnit.Pixel);
            }

            return image;
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
		            if (attributes != null)
		            {
		               attributes.Dispose();
                       attributes = null;
		            }

                    if (overlayImage != null)
                    {
                        overlayImage.Dispose();
                        overlayImage = null;
                    }
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

        ~OverlayImageProcessor()
		{
			Dispose (false);
		}
		
		#endregion

    }
}
