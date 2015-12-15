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
using System.Drawing.Imaging;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes an image by applying a filter
    /// </summary>
    public class FilterImageProcessor : ISingleImageProcessor, IDisposable
    {
        private readonly ILogger logger;
        private ImageAttributes attributes;
        private readonly FilterType filterType;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="filterType"></param>
        public FilterImageProcessor(ILogger logger, FilterType filterType)
        {
            this.logger = logger;
            this.filterType = filterType;

            logger.LogDebug("Creating FilterImageProcessor: " + filterType);
            
            //create some image attributes
            attributes = new ImageAttributes();

            //create the grayscale ColorMatrix
            var colorMatrix = FilterFactory.Create(filterType);
   
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession session, Image image)
        {            
            logger.LogInfo("Applying filter on image: " + filterType);

            if (image == null)
            {
                throw new ArgumentNullException("image", "image may not be null");
            }

            //get a graphics object from the image so we can draw on it
            using (var graphics = Graphics.FromImage(image))
            {
                var srcRectangle = new Rectangle(0, 0, image.Width, image.Height);
                graphics.DrawImage(image, srcRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
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
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

        ~FilterImageProcessor()
		{
			Dispose (false);
		}
		
		#endregion

    }
}
