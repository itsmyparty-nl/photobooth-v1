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
