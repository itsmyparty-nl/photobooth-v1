﻿#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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

using SixLabors.ImageSharp;

namespace com.prodg.photobooth.infrastructure.hardware
{
	/// <summary>
	/// The Printer interface
	/// </summary>
	public interface IPrinter: IHardwareController
	{
		/// <summary>
		/// Print a photo session
		/// </summary>
		/// <param name="image"></param>
		/// <param name="eventId"></param>
		/// <param name="sessionIndex"></param>
		void Print(Image image, string eventId, int sessionIndex);

		/// <summary>
		/// Retrieves the last printed image
		/// </summary>
		/// <returns>printed image</returns>
		Image? GetLastPrint();
	}
}