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

using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// Sink to store an image to disk in an image file
    /// </summary>
    public class ImageFileSink : ISingleImageProcessor
    {
        private readonly ILogger<ImageFileSink> _logger;
        private readonly string _fileName;
        private readonly IImageEncoder _encoder;

        public ImageFileSink(ILogger<ImageFileSink> logger, string fileName, IImageEncoder encoder)
        {
            _logger = logger;
            _fileName = fileName;
            _encoder = encoder;

            _logger.LogDebug("Creating ImageFileSink for {FileName} - {Encoder} ",fileName, encoder);
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession? session, Image image)
        {
            _logger.LogInformation("Storing file to disk {StoragePath} - {FileName}", session?.StoragePath, _fileName); 
            if (image == null) { throw new ArgumentNullException(nameof(image), "image may not be null"); }
            if (session == null) { throw new ArgumentNullException(nameof(session), "session may not be null"); }

            image.Save(Path.Combine(session.StoragePath, _fileName), _encoder);
            
            return image;
        }
    }
}
