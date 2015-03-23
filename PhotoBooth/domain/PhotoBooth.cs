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
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using com.prodg.photobooth.infrastructure.serialization;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoBooth: IDisposable
    {
        private ICamera camera;
        private CommandMessengerTransceiver commandMessenger;
        private ConsoleCommandReceiver consoleReceiver;

        public IHardware Hardware { get; private set; }

        public IPhotoBoothModel Model { get; private set; }

        public AutoResetEvent ShutdownRequested { get; private set; }

        public IPhotoBoothService Service { get; private set; }

        public ILogger Logger { get; private set; }

		public ISettings Settings { get; private set; }

        public PhotoBooth()
        {
            //Instantiate all classes
            ILogger logger = new NLogger();

            logger.LogInfo("Creating photobooth application");
            
            Settings = new Settings(logger);

            camera = new Camera(logger);
			commandMessenger = new CommandMessengerTransceiver(logger, Settings);
            consoleReceiver = new ConsoleCommandReceiver(logger);
            ShutdownRequested = new AutoResetEvent(false);

            IPrinter printer;
            ITriggerControl printControl;
            ITriggerControl printTwiceControl;

            ITriggerControl triggerControl = new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
            ITriggerControl powerControl = new RemoteTrigger(Command.Power, consoleReceiver, commandMessenger, logger);

            //Create a real printer or a stub depending on the settings
			if (!string.IsNullOrWhiteSpace(Settings.PrinterName))
            {
				printer = new NetPrinter(Settings, logger);
                printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
                printTwiceControl = new RemoteTrigger(Command.PrintTwice, commandMessenger, commandMessenger, logger);
            }
            else
            {
                printer = new PrinterStub(logger);
                printControl = new TriggerControlStub(Command.Print.ToString(), 300, logger);
                printTwiceControl = new TriggerControlStub(Command.PrintTwice.ToString(), null, logger);
            }

            Hardware = new Hardware(camera, printer, triggerControl, printControl, printTwiceControl,
                powerControl, logger);
  
            var serializer = new JsonStreamSerializer();
			var imageProcessor = new CollageImageProcessor(logger, Settings);
            
			Service = new PhotoBoothService(Hardware, imageProcessor, serializer, logger, Settings);

			Model = new PhotoBoothModel(Service, Hardware, logger, Settings);
            //Subscribe to the shutdown requested event 
            Model.ShutdownRequested += (sender, eventArgs) => ShutdownRequested.Set();
        }

        /// <summary>
        /// Start the photobooth
        /// </summary>
        public void Start()
        {
            Logger.LogInfo("Starting Photobooth application");
            //Start
            commandMessenger.Initialize();
            consoleReceiver.Initialize();
            Model.Start();
        }

        /// <summary>
        /// Stop the photobooth
        /// </summary>
        public void Stop()
        {
            Logger.LogInfo("Stopping Photobooth application");
            //Stop
            Model.Stop();
            consoleReceiver.DeInitialize();
            commandMessenger.DeInitialize();
        }

        #region IDisposable Implementation

		bool disposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
		    if (!disposed)
		    {
                Logger.LogInfo("Disposing Photobooth application");
		        if (disposing)
		        {
		            // Clean up managed objects
		            if (camera != null)
		            {
		                camera.Dispose();
		            }
		            if (commandMessenger != null)
		            {
		                commandMessenger.Dispose();
		            }
		            if (consoleReceiver != null)
		            {
		                consoleReceiver.Dispose();
		            }
		            if (Model != null)
		            {
		                Model.Dispose();
		            }
		            if (ShutdownRequested != null)
		            {
		                ShutdownRequested.Dispose();
		            }
		        }
		        camera = null;
                commandMessenger = null;
                consoleReceiver = null;
		        Hardware = null;
		        Model = null;
		        Logger = null;
		        Service = null;
		        ShutdownRequested = null;

		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

		~PhotoBooth()
		{
			Dispose (false);
		}
		
		#endregion


    }
}

