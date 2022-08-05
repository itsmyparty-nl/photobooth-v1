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

using com.prodg.photobooth.config;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes a collection of images into a single image
    /// </summary>
    public class CollageImageProcessor : IMultiImageProcessor
    {
        private readonly ILogger<CollageImageProcessor> _logger;
        private readonly ISettings _settings;

        private readonly Color backgroundColor = Color.White;

        public int RequiredImages { get; }

        public CollageImageProcessor(ILogger<CollageImageProcessor> logger, ISettings settings)
        {
            _logger = logger;
            _settings = settings;

            logger.LogDebug("Creating CollageImageProcessor");
            
            RequiredImages = settings.CollageGridHeight*settings.CollageGridWidth;
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession? session)
        {
	        if (session == null) { throw new ArgumentNullException(nameof(session));}

	        _logger.LogInformation("Creating a collage of {StoragePath}: {ImageCount} images",
		        session.StoragePath, session.ImageCount);
            
	        if (session.ImageCount != RequiredImages)
            {
                throw new InvalidOperationException($"Expected exactly {RequiredImages} images to process");
            }

            //Calculate output width & height while maintaining aspect ratio
            var collageWidth = CalculateCollageWidth(session.Images.First().Width);
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
            return finalImage;
        }

        private void DrawCroppedImageOnCollage(Image image, int croppedImageHeight, int imageIndex, Graphics g)
        {
            var srcStartY = image.Height == croppedImageHeight ? 0 : image.Height - croppedImageHeight - 1;
            var dstWidth = (int)Math.Floor(image.Width * _settings.CollageScalePercentage);
            var dstHeight = (int)Math.Floor((image.Height - srcStartY) * _settings.CollageScalePercentage);

            //Note: Start at double padding
            var dstStartX = (imageIndex%_settings.CollageGridWidth)*(dstWidth + _settings.CollagePaddingPixels) +
                         2*_settings.CollagePaddingPixels;
            var dstStartY = (imageIndex / _settings.CollageGridWidth) * (dstHeight + _settings.CollagePaddingPixels) +
                         2*_settings.CollagePaddingPixels;

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
                    ((croppedImageHeight*_settings.CollageGridHeight*_settings.CollageScalePercentage) +
                     (_settings.CollageGridHeight + 3)*_settings.CollagePaddingPixels);
            return collageHeight;
        }

        private int CalculateCollageWidth(int imageWidth)
        {
            //Gridwidth - 1 to get the padding in between. +4 to get double size borders outside the image
            return
                (int)
                    ((imageWidth*_settings.CollageGridWidth*_settings.CollageScalePercentage) +
                     (_settings.CollageGridWidth + 3)*_settings.CollagePaddingPixels);
        }

        private int CalculateCroppedImageHeight(int collageWidth)
        {
            //Calculate the desired height according to the aspect ratio
            var desiredCollageHeight = (int) (collageWidth/_settings.CollageAspectRatio);

            //totalHeight = (height*n*scale) + (n+3)*padpx
            //height = (totalHeight - padpx*(n + 3))/(n*scale)
            var croppedImageHeight =
                (int) ((desiredCollageHeight - _settings.CollagePaddingPixels*(_settings.CollageGridHeight + 3))/
                       (_settings.CollageGridHeight*_settings.CollageScalePercentage));
            return croppedImageHeight;
        }

    }
}
