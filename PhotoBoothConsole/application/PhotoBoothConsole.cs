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
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.application
{
	public static class PhotoBoothConsole
	{
		public static void Main (string[] args)
		{
		    ISettings settings = null;
            ILogger logger = new ConsoleLogger();
			IHardware hardware = new HardwareV1(logger);
            IImageProcessor imageProcessor = new CollageImageProcessor(logger,settings);
		 	var photoBooth = new PhotoBoothService(hardware, imageProcessor, logger, settings);

			//Start
			photoBooth.StartUp();
			//Wait until the photobooth is finished
			photoBooth.Finished.WaitOne();
			//Stop
			photoBooth.ShutDown();

			//Keep the application open
			Console.ReadLine();
		}
	}
}
