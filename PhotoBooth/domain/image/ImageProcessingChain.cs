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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;

namespace com.prodg.photobooth.domain.image
{
    public class ImageProcessingChain: IMultiImageProcessor
    {
        private readonly IList<ISingleImageProcessor> imagePreProcessors;
        private IMultiImageProcessor imageCombiner;
        private readonly IList<ISingleImageProcessor> imagePostProcessors;

        public int RequiredImages
        {
            get { return imageCombiner.RequiredImages; }
        }

        public ImageProcessingChain(ILogger logger, ISettings settings)
        {
            imagePreProcessors = new List<ISingleImageProcessor>
            {
                new FilterImageProcessor(logger, settings.Filter)
            };
            
            imageCombiner = new CollageImageProcessor(logger, settings);

            imagePostProcessors = new List<ISingleImageProcessor>
            {
                new OverlayImageProcessor(logger, "redbutton-logo2.png"),
                new ImageFileSink(logger, "collage.jpg", 90)
            };
        }

        public Image Process(PhotoSession session)
        {
            PreProcessImages(session);

            var combinedImage = imageCombiner.Process(session);

            return PostProcessCombinedImage(session, combinedImage);
        }

        private void PreProcessImages(PhotoSession session)
        {
            if (imagePreProcessors.Any())
            {
                for (var imageIndex = 0; imageIndex < session.ImageCount; imageIndex++)
                {
                    foreach (var imagePreProcessor in imagePreProcessors)
                    {
                        session.Images[imageIndex] = imagePreProcessor.Process(session, session.Images[imageIndex]);
                    }
                }
            }
        }

        private Image PostProcessCombinedImage(PhotoSession session, Image combinedImage)
        {
            if (imagePostProcessors.Any())
            {
                foreach (var combinedImagePostProcessor in imagePostProcessors)
                {
                    combinedImage = combinedImagePostProcessor.Process(session, combinedImage);
                }
            }
            return combinedImage;
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
		            if (imageCombiner != null)
		            {
		               imageCombiner.Dispose();
                       imageCombiner = null;
		            }

		            if (imagePreProcessors != null && imagePreProcessors.Any())
		            {
		                foreach (var singleImageProcessor in imagePreProcessors)
		                {
		                    singleImageProcessor.Dispose();
		                }
                        imagePreProcessors.Clear();
		            }

		            if (imagePostProcessors != null && imagePostProcessors.Any())
		            {
		                foreach (var singleImageProcessor in imagePostProcessors)
		                {
		                    singleImageProcessor.Dispose();
		                }
                        imagePostProcessors.Clear();
		            }
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

        ~ImageProcessingChain()
		{
			Dispose (false);
		}
		
		#endregion
   }
}
