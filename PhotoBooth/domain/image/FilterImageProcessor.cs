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

using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes an image by applying a filter
    /// </summary>
    public class FilterImageProcessor : ISingleImageProcessor
    {
        private readonly ILogger<FilterImageProcessor> _logger;
        private readonly FilterType _filterType;
        private readonly ColorMatrix _colorMatrix;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="filterType"></param>
        public FilterImageProcessor(ILogger<FilterImageProcessor> logger, FilterType filterType)
        {
            _logger = logger;
            _filterType = filterType;

            //_logger.LogDebug("Creating FilterImageProcessor for {FilterType}", filterType);
            
            //create the grayscale ColorMatrix
            _colorMatrix = FilterFactory.Create(filterType);
        }

        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(PhotoSession? session, Image image)
        {            
            //_logger.LogInformation("Applying filter {FilterType} on image", _filterType);

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "image may not be null");
            }
            
            image.Mutate(context => context.Filter(_colorMatrix));
            
            return image;
        }
    }
}
