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
using System.Threading;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.application
{
	public static class PhotoBoothConsole
	{
        private static readonly ManualResetEvent ShutdownRequested = new ManualResetEvent(false);
        
        public static void Main (string[] args)
		{
		    //Instantiate all classes
			ILogger logger = new ConsoleLogger();
			ISettings settings = new Settings(logger);

            var camera = new Camera(logger);
			var printer = new NetPrinter(settings,logger);
            var commandMessenger = new CommandMessengerTransceiver(logger, settings);
            var triggerControl = new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
            var printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
            var printTwiceControl = new RemoteTrigger(Command.PrintTwice, commandMessenger, commandMessenger, logger);
            var powerControl = new RemoteTrigger(Command.Power, commandMessenger, commandMessenger, logger);
			IHardware hardware = new Hardware(camera,printer,triggerControl,printControl,printTwiceControl,powerControl,logger);

            IImageProcessor imageProcessor = new CollageImageProcessor(logger,settings);
		 	IPhotoBoothService photoBoothService = new PhotoBoothService(hardware, imageProcessor, logger, settings);
		    IPhotoBoothModel photoBooth = new PhotoBoothModel(photoBoothService, hardware, logger);

            //Subscribe to the shutdown requested event 
            photoBooth.ShutdownRequested += (sender, eventArgs) => ShutdownRequested.Set();

			//Start
			photoBooth.Start();
			//Wait until the photobooth is finished
			ShutdownRequested.WaitOne();
			//Stop
			photoBooth.Stop();

			//Keep the application open to view any console logging
			Console.ReadLine();
		}
	}
}
