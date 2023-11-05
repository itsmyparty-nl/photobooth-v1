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

using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using CommandMessenger.TransportLayer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoBooth: IDisposable
    {
        public IPhotoBoothModel Model { get; }

        private readonly IHardware _hardware;
        private readonly ICommandMessengerTransceiver _commandMessenger;
        private readonly IConsoleCommandReceiver _consoleReceiver;
        private readonly IRemoteTriggerService _triggerService;
        private readonly ILogger<PhotoBooth> _logger;
        
        public PhotoBooth(IPhotoBoothModel model, IHardware hardware, ICommandMessengerTransceiver commandMessenger,
            IConsoleCommandReceiver consoleReceiver, IRemoteTriggerService triggerService, ILogger<PhotoBooth> logger)
        {
            Model = model;
            _hardware = hardware;
            _commandMessenger = commandMessenger;
            _consoleReceiver = consoleReceiver;
            _triggerService = triggerService;
            _logger = logger;

        }

        
        /// <summary>
        /// Start the photobooth
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Starting PhotoBooth");
            //Start
            _triggerService.Register(_hardware.TriggerControl);
            _triggerService.Register(_hardware.PowerControl);
            _triggerService.Register(_hardware.PrintControl);
            _triggerService.Register(_hardware.PrintTwiceControl);
            
            _commandMessenger.Initialize();
            _consoleReceiver.Initialize();
            Model.Start();
        }

        /// <summary>
        /// Stop the photobooth
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping PhotoBooth");
            //Stop
            Model.Stop();
            _triggerService.DeRegister(_hardware.TriggerControl);
            _triggerService.DeRegister(_hardware.PowerControl);
            _triggerService.DeRegister(_hardware.PrintControl);
            _triggerService.DeRegister(_hardware.PrintTwiceControl);
            
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
                _logger.LogInformation("Disposing PhotoBooth");
                if (disposing)
                {
                    // Clean up managed objects
                    Model.Dispose();
                    
                    _commandMessenger.Dispose();
                    
                    _consoleReceiver.Dispose();
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

