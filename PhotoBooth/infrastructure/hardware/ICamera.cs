/*
 *Copyright 2014 Patrick Bronneberg
 * 
*/

using System;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public interface ICamera: IDisposable
	{
		/// <summary>
		/// The identifier (model) of the camera
		/// </summary>
		string Id {get;}
		
		/// <summary>
		/// Initialize the camera and retrieve settings
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		bool Initialize();
		
		/// <summary>
		/// Capture a single shot
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/> to indicate whether the capture succeeded 
		/// </returns>
		/// <remarks>In case the shot did not succeed, a re-initialize of
		/// the camera is forced</remarks>
		bool Capture(string capturePath);
		
		/// <summary>
		/// Clean any data on the camera
		/// </summary>
		void Clean();
	}
}
