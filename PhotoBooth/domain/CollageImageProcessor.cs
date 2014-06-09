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

using System.Drawing.Drawing2D;
using com.prodg.photobooth.common;
using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using com.prodg.photobooth.config;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// An image processor processes a collection of images into a single image
    /// </summary>
    public class CollageImageProcessor : IImageProcessor, IDisposable
    {
        private readonly ILogger logger;
        private readonly ISettings settings;

        private readonly Color backgroundColor = Color.White;
        private const string CollageFilename = "Collage.jpg";
        private ImageAttributes attributes;

        /// <summary>
        /// <see cref="IImageProcessor.RequiredImages"/>
        /// </summary>
        public int RequiredImages { get; private set; }

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public CollageImageProcessor(ILogger logger, ISettings settings)
        {
            this.logger = logger;
            this.settings = settings;
            //create some image attributes
            attributes = new ImageAttributes();

            RequiredImages = settings.CollageGridHeight*settings.CollageGridWidth;

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new[] {.3f, .3f, .3f, 0, 0},
                    new[] {.59f, .59f, .59f, 0, 0},
                    new[] {.11f, .11f, .11f, 0, 0},
                    new[] {0, 0, 0, 1f, 0},
                    new[] {0, 0, 0, 0, 1f}
                });

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession session)
        {
            logger.LogInfo(string.Format("Processing {0}: {1} images", session.StoragePath, session.ImageCount));
            if (session.ImageCount != RequiredImages)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Expected exactly {0} images to process", RequiredImages));
            }

            //Calculate output width & height while maintaining aspect ratio
            var collageWidth = CalculateCollageWidth(session.Images[0].Width);
            var croppedImageHeight = CalculateCroppedImageHeight(collageWidth);
            var collageHeight = CalculateCollageHeight(croppedImageHeight);

            //create a bitmap to hold the combined image
            Image finalImage = new Bitmap(collageWidth, collageHeight);
            //get a graphics object from the image so we can draw on it
            using (var graphics = Graphics.FromImage(finalImage))
            {
                //set background color
                graphics.Clear(backgroundColor);

                //go through each image and draw it on the final image
                var imageIndex = 0;
                foreach (var currentImage in session.Images)
                {
                    DrawCroppedImageOnCollage(currentImage, croppedImageHeight, imageIndex, graphics);
                    imageIndex++;
                }
            }

            finalImage.Save(Path.Combine(session.StoragePath, CollageFilename), ImageFormat.Jpeg);
            return finalImage;
        }

        private void DrawCroppedImageOnCollage(Image image, int croppedImageHeight, int imageIndex, Graphics g)
        {
            var srcStartY = image.Height == croppedImageHeight ? 0 : image.Height - croppedImageHeight - 1;
            var dstWidth = (int)Math.Floor(image.Width * settings.CollageScalePercentage);
            var dstHeight = (int)Math.Floor((image.Height - srcStartY) * settings.CollageScalePercentage);

            //Note: Start at double padding
            var dstStartX = (imageIndex%settings.CollageGridWidth)*(dstWidth + settings.CollagePaddingPixels) +
                         2*settings.CollagePaddingPixels;
            var dstStartY = (imageIndex / settings.CollageGridWidth) * (dstHeight + settings.CollagePaddingPixels) +
                         2*settings.CollagePaddingPixels;

            //draw the original image on the new image using the grayscale color matrix
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle(dstStartX, dstStartY, dstWidth, dstHeight),
                0, srcStartY, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }

        private int CalculateCollageHeight(int croppedImageHeight)
        {
            //Gridwidth - 1 to get the padding in between. +4 to get double size borders outside the image
            var collageHeight =
                (int)
                    ((croppedImageHeight*settings.CollageGridHeight*settings.CollageScalePercentage) +
                     (settings.CollageGridHeight + 3)*settings.CollagePaddingPixels);
            return collageHeight;
        }

        private int CalculateCollageWidth(int imageWidth)
        {
            //Gridwidth - 1 to get the padding in between. +4 to get double size borders outside the image
            return
                (int)
                    ((imageWidth*settings.CollageGridWidth*settings.CollageScalePercentage) +
                     (settings.CollageGridWidth + 3)*settings.CollagePaddingPixels);
        }

        private int CalculateCroppedImageHeight(int collageWidth)
        {
            //Calculate the desired height according to the aspect ratio
            var desiredCollageHeight = (int) (collageWidth/settings.CollageAspectRatio);

            //totalHeight = (height*n*scale) + (n+3)*padpx
            //height = (totalHeight - padpx*(n + 3))/(n*scale)
            var croppedImageHeight =
                (int) ((desiredCollageHeight - settings.CollagePaddingPixels*(settings.CollageGridHeight + 3))/
                       (settings.CollageGridHeight*1f));
            return croppedImageHeight;
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
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

        ~CollageImageProcessor()
		{
			Dispose (false);
		}
		
		#endregion

    }
}
