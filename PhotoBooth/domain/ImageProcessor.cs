/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System.Collections.Generic;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.domain
{
	/// <summary>
	/// An image processor processes a collection of images into a single image
	/// </summary>
	public class ImageProcessor: IImageProcessor
	{
	    private readonly ILogger logger;
        public ImageProcessor(ILogger logger)
        {
            this.logger = logger;
        }
        
        /// <summary>
		/// Processes the images into a single image
		/// </summary>
		/// <param name="id">The identifier of the image</param>
		/// <param name="images">
		/// A <see cref="IList<System.String>" />
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> containing the path of the result image
		/// </returns>
		public string Process(string id, IList<string> images)
		{
		    logger.LogInfo(string.Format("Processing {0}: {1} images", id, images.Count));
            return images[0];
		}
	}
}
