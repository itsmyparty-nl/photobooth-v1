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
using System.Globalization;
using System.Threading;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using com.prodg.photobooth.infrastructure.serialization;
using CommandMessenger.TransportLayer;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoBooth : IDisposable
    {
        private ICamera camera;
        private CommandMessengerTransceiver commandMessenger;
        private ConsoleCommandReceiver consoleReceiver;
        private ITransport transport;

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

            CreateTransport(logger);

            //transport = new StubbedTransport ();
            commandMessenger = new CommandMessengerTransceiver(logger, transport);
            consoleReceiver = new ConsoleCommandReceiver(logger);
            ShutdownRequested = new AutoResetEvent(false);

            ITriggerControl printControl;
            ITriggerControl triggerControl = CreateTriggerControl(logger);
            ITriggerControl printTwiceControl;
            IPrinter printer = CreatePrinterControls(logger, out printControl, out printTwiceControl);
            ITriggerControl powerControl = new RemoteTrigger(Command.Power, consoleReceiver, commandMessenger, logger);

            Hardware = new Hardware(camera, printer, triggerControl, printControl, printTwiceControl,
                powerControl, logger);

            var serializer = new JsonStreamSerializer(logger);
            var imageProcessor = new CollageImageProcessor(logger, Settings);

            Service = new PhotoBoothService(Hardware, imageProcessor, serializer, logger, Settings);

            Model = new PhotoBoothModel(Service, Hardware, logger, Settings);
            //Subscribe to the shutdown requested event 
            Model.ShutdownRequested += (sender, eventArgs) => ShutdownRequested.Set();
        }

        private ITriggerControl CreateTriggerControl(ILogger logger)
        {
            if (!String.IsNullOrWhiteSpace(Settings.SerialPortName))
            {
                return new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
            }
            return new RemoteTrigger(Command.Trigger, consoleReceiver, commandMessenger, logger);
        }

        private IPrinter CreatePrinterControls(ILogger logger, out ITriggerControl printControl,
            out ITriggerControl printTwiceControl)
        {
            IPrinter printer;
            if (!string.IsNullOrWhiteSpace(Settings.PrinterName))
            {
                logger.LogDebug(String.Format(CultureInfo.InvariantCulture, "Creating Printer {0}", Settings.PrinterName));
                printer = new NetPrinter(Settings, logger);
                if (!String.IsNullOrWhiteSpace(Settings.SerialPortName))
                {
                    printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
                    printTwiceControl = new RemoteTrigger(Command.PrintTwice, commandMessenger, commandMessenger, logger);
                }
                else
                {
                    printControl = new RemoteTrigger(Command.Print, consoleReceiver, commandMessenger, logger);
                    printTwiceControl = new RemoteTrigger(Command.PrintTwice, consoleReceiver, commandMessenger, logger);
                }
            }
            else
            {
                printer = new PrinterStub(logger);
                printControl = new TriggerControlStub(Command.Print.ToString(), 300, logger);
                printTwiceControl = new TriggerControlStub(Command.PrintTwice.ToString(), null, logger);
            }
            return printer;
        }

        private void CreateTransport(ILogger logger)
        {

            // Create Serial Port object
            // Note that for some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.
            if (!string.IsNullOrWhiteSpace(Settings.SerialPortName))
            {
                logger.LogDebug(String.Format(CultureInfo.InvariantCulture, "Creating serial transport {0}:{1}:{2}",
                    Settings.SerialPortName, Settings.SerialPortBaudRate, Settings.SerialPortDtrEnable));
                transport = new SerialTransport
                {
                    CurrentSerialSettings =
                    {
                        PortName = Settings.SerialPortName,
                        BaudRate = Settings.SerialPortBaudRate,
                        DtrEnable = Settings.SerialPortDtrEnable
                    }
                };
            }
            else
            {
                logger.LogDebug("Creating stubbed transport");
                transport = new StubbedTransport();
            }
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

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
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
            Dispose(false);
        }

        #endregion
    }
}

