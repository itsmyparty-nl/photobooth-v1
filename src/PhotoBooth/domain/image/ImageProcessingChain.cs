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

using System.Diagnostics;
using com.prodg.photobooth.config;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace com.prodg.photobooth.domain.image
{
    public class ImageProcessingChain: IMultiImageProcessor
    {
	    private readonly ILogger<ImageProcessingChain> _logger;
	    private readonly IList<ISingleImageProcessor> _imagePreProcessors;
        private readonly IMultiImageProcessor _imageCombiner;
        private readonly IList<ISingleImageProcessor> _imagePostProcessors;

        public int RequiredImages => _imageCombiner.RequiredImages;

        public ImageProcessingChain(ILoggerFactory loggerFactory, ILogger<ImageProcessingChain> logger, ISettings settings)
        {
	        _logger = logger;
	        _imagePreProcessors = new List<ISingleImageProcessor>
            {
                new FilterImageProcessor(new Logger<FilterImageProcessor>(loggerFactory), settings.Filter)
            };

            // if (!string.IsNullOrWhiteSpace(settings.FixedImageFilename))
            // {
	           //  _imageCombiner = new FixedImageCollageImageProcessor(
		          //   new Logger<FixedImageCollageImageProcessor>(loggerFactory), settings, settings.FixedImageFilename);
            // }
            // else
            // {
	            _imageCombiner = new CollageImageProcessor(new Logger<CollageImageProcessor>(loggerFactory), settings);
            //}

            _imagePostProcessors = new List<ISingleImageProcessor>();
            if (!string.IsNullOrWhiteSpace(settings.OverlayImageFilename))
            {
	            _imagePostProcessors.Add(new OverlayImageProcessor(new Logger<OverlayImageProcessor>(loggerFactory),
		            settings.OverlayImageFilename));
            }

            _imagePostProcessors.Add(new ImageFileSink(new Logger<ImageFileSink>(loggerFactory), "collage.jpg",
	            new JpegEncoder()));
        }

        public Image? Process(PhotoSession? session)
        {
	        _logger.LogInformation("Process session");
	        if (session == null)
	        {
		        _logger.LogWarning("Session processings is requested for a NULL session");
		        Debug.Assert(session == null, "Session processings is requested for a NULL session");
		        return null;
	        }
            
	        PreProcessImages(session);

            var combinedImage = _imageCombiner.Process(session);

            return PostProcessCombinedImage(session, combinedImage!);
        }

        private void PreProcessImages(PhotoSession session)
        {
	        _logger.LogInformation("Preprocessing images");
	        if (_imagePreProcessors.Any())
            {
                for (var imageIndex = 0; imageIndex < session!.ImageCount; imageIndex++)
                {
                    foreach (var imagePreProcessor in _imagePreProcessors)
                    {
                        session.Images[imageIndex] = imagePreProcessor.Process(session, session.Images[imageIndex]);
                    }
                }
            }
        }

        private Image PostProcessCombinedImage(PhotoSession session, Image combinedImage)
        {
            if (_imagePostProcessors.Any())
            {
                foreach (var combinedImagePostProcessor in _imagePostProcessors)
                {
                    combinedImage = combinedImagePostProcessor.Process(session, combinedImage);
                }
            }
            return combinedImage;
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
			if (_disposed) return;
			if (disposing)
			{
				// Clean up managed objects
				_imageCombiner.Dispose();

				DisposeImageProcessors(_imagePreProcessors);
				DisposeImageProcessors(_imagePostProcessors);
			}
			// clean up any unmanaged objects
			_disposed = true;
		}

		private static void DisposeImageProcessors(IList<ISingleImageProcessor> processors)
		{
			if (!processors.Any()) return;
			foreach (var singleImageProcessor in processors)
			{
				(singleImageProcessor as IDisposable)?.Dispose();
			}
			processors.Clear();
		}

		~ImageProcessingChain()
		{
			Dispose (false);
		}
		
		#endregion
   }
}
