/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;

namespace com.prodg.photobooth.common
{
	public class ConsoleLogger: ILogger
	{
		public void LogInfo(string logString)
		{
			Console.WriteLine(logString);
		}
		
		public void LogWarning(string logString)
		{
			Console.WriteLine("WARNING: "+logString);
		}

		public void LogError(string logString)
		{
			Console.WriteLine("WARNING: "+logString);
		}

		public void LogException(string logString, Exception exception)
		{
			Console.WriteLine("EXCEPTION: "+logString+", "+exception.Message);
			Console.WriteLine(exception.ToString());
		}
	}
}