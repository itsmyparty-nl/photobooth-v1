/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;

namespace com.prodg.photobooth.common
{
	public interface ILogger
	{
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