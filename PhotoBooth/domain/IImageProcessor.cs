/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System.Collections.Generic;

namespace com.prodg.photobooth.domain
{
	/// <summary>
	/// An image processor processes a collection of images into a single image
	/// </summary>
	public interface IImageProcessor
	{
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
		string Process(string id, IList<string> images);
	}
}
