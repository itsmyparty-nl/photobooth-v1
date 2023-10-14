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

using System.Diagnostics;
using com.prodg.photobooth.config;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

//using Color = System.Drawing.Color;
//using Rectangle = System.Drawing.Rectangle;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes a collection of images into a single image
    /// </summary>
    public class CollageImageProcessor : IMultiImageProcessor
    {
        private readonly ILogger<CollageImageProcessor> _logger;
        private readonly ISettings _settings;

        
        private readonly Configuration _configuration = new(
            new PngConfigurationModule(),
            new JpegConfigurationModule(),
            new BmpConfigurationModule());
       
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
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

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
            Image<Rgb24> finalImage = new Image<Rgb24>(_configuration, collageWidth, collageHeight);
            finalImage.Mutate(x => x.Fill(Color.White));
            //go through each image and draw it on the final image
            var imageIndex = 0;
            foreach (var currentImage in session.Images)
            {
                using var image = currentImage.CloneAs<Rgb24>();
                
                var dstWidth = (int)Math.Floor(image.Width * _settings.CollageScalePercentage);
                image.Mutate(x => x.Resize(dstWidth, 0, KnownResamplers.Lanczos3));

                var dstStartX = (imageIndex % _settings.CollageGridWidth) *
                                (image.Width + _settings.CollagePaddingPixels) +
                                2 * _settings.CollagePaddingPixels;
                var dstStartY = (imageIndex / _settings.CollageGridWidth) *
                                (image.Height + _settings.CollagePaddingPixels) +
                                2 * _settings.CollagePaddingPixels;

                finalImage.Mutate(x =>
                {
                    Debug.Assert(image != null, nameof(image) + " != null");
                    x.DrawImage(image, new Point(dstStartX, dstStartY), 1);
                });
                imageIndex++;
            }

            return finalImage;
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

        #region IDisposable Implementation

        bool _disposed;

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        private void Dispose (bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clean up managed objects
                }
                // clean up any unmanaged objects
                _disposed = true;
            }
        }

        ~CollageImageProcessor()
        {
            Dispose (false);
        }
		
        #endregion

        
    }
}
