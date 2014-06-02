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
	public class ConsoleLogger: ILogger
	{
        
        public void LogDebug(string logString)
        {
            Console.WriteLine("DEBUG: " + logString);
        }
        
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
			Console.WriteLine("ERROR: "+logString);
		}

		public void LogException(string logString, Exception exception)
		{
			Console.WriteLine("EXCEPTION: "+logString+", "+exception.Message);
			Console.WriteLine(exception.ToString());
		}
	}
}