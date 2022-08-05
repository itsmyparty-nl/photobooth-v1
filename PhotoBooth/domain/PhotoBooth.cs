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

using com.prodg.photobooth.config;
using com.prodg.photobooth.domain.image;
using com.prodg.photobooth.domain.offload;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using com.prodg.photobooth.infrastructure.serialization;
using CommandMessenger.TransportLayer;
using ItsMyParty.Photobooth.Api;
using ItsMyParty.Photobooth.Client;
using Microsoft.Extensions.Logging;
using Configuration = SixLabors.ImageSharp.Configuration;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoBooth : IDisposable
    {
        private readonly ICamera _camera;
        private readonly CommandMessengerTransceiver _commandMessenger;
        private readonly ConsoleCommandReceiver _consoleReceiver;
        private ITransport _transport;

        public IHardware Hardware { get; }

        public IPhotoBoothModel Model { get; }

        public AutoResetEvent ShutdownRequested { get; }

        public IPhotoBoothService Service { get; }

        public IPhotoboothOffloader Offloader { get; }

        private ILogger<PhotoBooth> _logger;

        public ISettings Settings { get; }

        public PhotoBooth()
        {
            //Instantiate all classes
            var factory = new LoggerFactory();
            _logger = new Logger<PhotoBooth>(factory);
            _logger.LogInformation("Creating photobooth application");


            var serializer = new JsonStreamSerializer(new Logger<JsonStreamSerializer>(factory));

            Settings = new Settings(new Logger<Settings>(factory));

            _camera = new Camera(new Logger<Camera>(factory));

            CreateTransport();

            //transport = new StubbedTransport ();
            _commandMessenger =
                new CommandMessengerTransceiver(new Logger<CommandMessengerTransceiver>(factory), _transport!);
            _consoleReceiver = new ConsoleCommandReceiver(new Logger<ConsoleCommandReceiver>(factory));
            ShutdownRequested = new AutoResetEvent(false);

            if (Settings.OffloadSessions)
            {
                var config = Configuration.Default;
                config.ApiClient = new ApiClient(Settings.OffloadAddress);
                config.Timeout = 20000;
                var sessionApi = new SessionApiApi(config);
                var shotApi = new ShotApiApi(config);
                var offloadContextFileHandler =
                    new OffloadContextFileHandler(serializer, new Logger<OffloadContextFileHandler>(factory));
                Offloader = new PhotoboothOffloader(sessionApi, shotApi, Settings, new Logger<PhotoboothOffloader>(factory),
                    offloadContextFileHandler);
            }
            else
            {
                Offloader = new OffloadStub();
            }

            ITriggerControl printControl;
            ITriggerControl triggerControl = CreateTriggerControl(factory);
            ITriggerControl printTwiceControl;
            IPrinter printer = CreatePrinterControls(Logger<>, out printControl, out printTwiceControl);
            ITriggerControl powerControl = new RemoteTrigger(Command.Power, _consoleReceiver, _commandMessenger,
                new Logger<RemoteTrigger>(factory));

            Hardware = new Hardware(_camera, printer, triggerControl, printControl, printTwiceControl,
                powerControl, new Logger<Hardware>(factory));

            var imageProcessor = new ImageProcessingChain(factory, new Logger<ImageProcessingChain>(factory), Settings);

            Service = new PhotoBoothService(Hardware, imageProcessor, serializer,
                new Logger<PhotoBoothService>(factory), Settings, Offloader);

            Model = new PhotoBoothModel(Service, Hardware, new Logger<PhotoBoothModel>(factory), Settings);

            //Subscribe to the shutdown requested event 
            Model.ShutdownRequested += (_, _) => ShutdownRequested.Set();
        }

        private ITriggerControl CreateTriggerControl(ILoggerFactory factory)
        {
            if (!String.IsNullOrWhiteSpace(Settings.SerialPortName))
            {
                return new RemoteTrigger(Command.Trigger, _commandMessenger, _commandMessenger,
                    new Logger<RemoteTrigger>(factory));
            }

            return new RemoteTrigger(Command.Trigger, _consoleReceiver, _commandMessenger,
                new Logger<RemoteTrigger>(factory));
        }

        private IPrinter CreatePrinterControls(ILoggerFactory factory, out ITriggerControl printControl,
            out ITriggerControl printTwiceControl)
        {
            IPrinter printer;
            if (!string.IsNullOrWhiteSpace(Settings.PrinterName))
            {
                _logger.LogDebug("Creating Printer {PrinterName}", Settings.PrinterName);
                printer = new NetPrinter(Settings, new Logger<NetPrinter>(factory));

                var triggerLogger = new Logger<RemoteTrigger>(factory);
                if (!String.IsNullOrWhiteSpace(Settings.SerialPortName))
                {
                    printControl = new RemoteTrigger(Command.Print, _commandMessenger, _commandMessenger, triggerLogger);
                    printTwiceControl = new RemoteTrigger(Command.PrintTwice, _commandMessenger, _commandMessenger, triggerLogger);
                }
                else
                {
                    printControl = new RemoteTrigger(Command.Print, _consoleReceiver, _commandMessenger, triggerLogger);
                    printTwiceControl = new RemoteTrigger(Command.PrintTwice, _consoleReceiver, _commandMessenger, triggerLogger);
                }
            }
            else
            {
                printer = new PrinterStub(new Logger<PrinterStub>(factory));
                printControl = new TriggerControlStub(Command.Print.ToString(), 60,
                    new Logger<TriggerControlStub>(factory));
                printTwiceControl = new TriggerControlStub(Command.PrintTwice.ToString(), null,
                    new Logger<TriggerControlStub>(factory));
            }
            return printer;
        }

        private void CreateTransport()
        {
            // Create Serial Port object
            // Note that for some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.
            if (!string.IsNullOrWhiteSpace(Settings.SerialPortName))
            {
                _logger.LogDebug("Creating serial transport {SerialPortName}:{SerialPortBaudRate}:{SerialPortDtrEnable}",
                    Settings.SerialPortName, Settings.SerialPortBaudRate, Settings.SerialPortDtrEnable);
                _transport = new SerialTransport
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
                _logger.LogDebug("Creating stubbed transport");
                _transport = new StubbedTransport();
            }
        }

        /// <summary>
        /// Start the photobooth
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Starting Photobooth application");
            //Start
            _commandMessenger.Initialize();
            _consoleReceiver.Initialize();
            Model.Start();
        }

        /// <summary>
        /// Stop the photobooth
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping Photobooth application");
            //Stop
            Model.Stop();
            _consoleReceiver.DeInitialize();
            _commandMessenger.DeInitialize();
        }

        #region IDisposable Implementation

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _logger.LogInformation("Disposing Photobooth application");
                if (disposing)
                {
                    // Clean up managed objects

                    _commandMessenger.Dispose();
                    _consoleReceiver.Dispose();
                    Model.Dispose();
                    ShutdownRequested.Dispose();
                }

                // clean up any unmanaged objects
                _disposed = true;
            }
        }

        ~PhotoBooth()
        {
            Dispose(false);
        }

        #endregion
    }
}

