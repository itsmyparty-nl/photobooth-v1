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

namespace com.prodg.photobooth.common
{
	public interface ILogger
	{
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="logString">
        /// A <see cref="System.String"/>
        /// </param>
        void LogDebug(string logString);

        /// <summary>
		/// Log an info message
		/// </summary>
		/// <param name="logString">
		/// A <see cref="System.String"/>
		/// </param>
		void LogInfo(string logString);
		
		/// <summary>
		/// Log a warning message
		/// </summary>
		/// <param name="logString">
		/// A <see cref="System.String"/>
		/// </param>
		void LogWarning(string logString);

		/// <summary>
		/// Log an error message
		/// </summary>
		/// <param name="logString">
		/// A <see cref="System.String"/>
		/// </param>
		void LogError(string logString);

		/// <summary>
		/// Log an exception
		/// </summary>
		/// <param name="logString">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="exception">
		/// A <see cref="Exception"/>
		/// </param>
		void LogException(string logString, Exception exception);
	}
}