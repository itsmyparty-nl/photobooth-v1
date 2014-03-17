/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
using com.prodg.photobooth.common;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.application
{
	public static class PhotoBoothConsole
	{
		public static void Main (string[] args)
		{
			ILogger logger = new ConsoleLogger();
		    IHardware hardware = new HardwareV1(logger);
            var photoBooth = new PhotoBooth(hardware, logger);
            
            //Start
            photoBooth.Start();
            //Wait until the photobooth is finished
		    photoBooth.Finished.WaitOne();
            //Stop
            photoBooth.Stop();

		    //Keep the application open
            Console.ReadLine();
		}
	}
}

