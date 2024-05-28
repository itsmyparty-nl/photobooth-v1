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
using com.prodg.photobooth.infrastructure.hardware;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain
{
	/// <summary>
    /// The Photobooth application model.
    /// <see cref="IPhotoBoothModel"/>
    /// </summary>
    public class PhotoBoothModel : IPhotoBoothModel
    {
        private readonly ILogger _logger;
        private readonly IHardware _hardware;
        private readonly IPhotoBoothService _service;
        private readonly SemaphoreSlim _sessionLock;
	    private readonly ISettings _settings;
		private PhotoSession? _currentSession;

        /// <summary>
        /// Event to signal that shutdown is requested
        /// </summary>
        /// <remarks>This event is added to provide a single location for handling all hardware controls, while keeping
        /// the responsibility of stopping the model at the application class</remarks>
        public event EventHandler? ShutdownRequested;

		/// <summary>
		/// Occurs when error occurred.
		/// </summary>
		public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hardware"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PhotoBoothModel(IPhotoBoothService service, IHardware hardware, ILogger<PhotoBoothModel> logger, ISettings settings)
        {
            this._hardware = hardware;
            this._logger = logger;
            this._service = service;
            this._settings = settings;
            _sessionLock = new SemaphoreSlim (1);
            
            logger.LogDebug("Creating PhotoBooth Model");

            _currentSession = null;
        }

        /// <summary>
        /// Start the photobooth model
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Starting Photobooth application model for event {EventId}", _settings.EventId);

            //Register events
            _hardware.Camera.StateChanged += OnCameraStateChanged!;
            _hardware.TriggerControl.Fired += OnTriggerControlTriggered!;
            _hardware.PrintControl.Fired += OnPrintControlTriggered!;
            _hardware.PrintTwiceControl.Fired += OnPrintTwiceControlTriggered!;
            _hardware.PowerControl.Fired += OnPowerControlTriggered!;
            
            //Acquire the hardware
            _hardware.Acquire();

            //Start with only the power control armed
            _hardware.PowerControl.Arm();
        }

        /// <summary>
        /// Stop the photobooth model
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping Photobooth application model");
            
            //Release the hardware
            _hardware.Release();
            
            //Unsubscribe from all hardware events
            _hardware.Camera.StateChanged -= OnCameraStateChanged!;
            _hardware.TriggerControl.Fired -= OnTriggerControlTriggered!;
            _hardware.PrintControl.Fired -= OnPrintControlTriggered!;
            _hardware.PrintTwiceControl.Fired -= OnPrintTwiceControlTriggered!;
            _hardware.PowerControl.Fired -= OnPowerControlTriggered!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This should be the only location where release/arm of the trigger is performed</remarks>
        void OnCameraStateChanged(object sender, CameraStateChangedEventArgs e)
        {
            if (e.NewState)
            {
                Task.Run(() => _hardware.TriggerControl.Arm());
            }
            else
            {
                Task.Run(() => _hardware.TriggerControl.Release());
            }
        }

        private void OnPowerControlTriggered(object sender, TriggerControlEventArgs e)
        {
            _logger.LogInformation("Power control fired");

            try
            {
                //Trigger a shutdown request.
                //Do not call stop directly since this is the responsibility of the application
                ShutdownRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                //Log the exception
                _logger.LogError(ex, "Error while handling shutdown");
                //Rethrow since no mitigation is possible
                throw;
            }
        }

        private async void OnPrintControlTriggered(object sender, TriggerControlEventArgs e)
		{
			_logger.LogInformation("Print control fired");
           
			//lock both buttons but only indicate that the first is printing
			var lockId = _hardware.PrintControl.Lock (true);
            var twiceLockId = _hardware.PrintTwiceControl.Lock(false);
            try
            {
                try
                {
                    _sessionLock.Wait();
                    //Print
                    if (_currentSession != null)
                    {
                        //Get the last sesion from the queue and print it
                        await _service.Print(_currentSession);
                    }
                    else
                    {
                        _logger.LogWarning("Nothing to print");
                    }
                }
                finally
                {
                    _sessionLock.Release();
                }

                //Wait until releasing the control to show that printing is busy
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.PrintDurationMs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while printing");
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while printing"));
                }
            }
            finally
            {
                //always reset the control to its initial state
                _hardware.PrintControl.Unlock(lockId);
                _hardware.PrintTwiceControl.Unlock(twiceLockId);
            }
		}

        private async void OnPrintTwiceControlTriggered(object sender, TriggerControlEventArgs e)
		{
			_logger.LogInformation("Print twice control fired");

			//Release the print button to prevent printing twice
			//hardware.PrintControl.Release ();
			var lockId = _hardware.PrintTwiceControl.Lock (true);
            var singleLockId = _hardware.PrintControl.Lock(false);

            try
            {
                try
                {

                    _sessionLock.Wait();
                    if (_currentSession != null)
                    {
                        //Get the last sesion from the queue and print it
                        await _service.Print(_currentSession);
                        await _service.Print(_currentSession);
                    }
                    else
                    {
                        _logger.LogInformation("Nothing to print");
                        return;
                    }

                }
                finally
                {
                    _sessionLock.Release();
                }
                //Wait until releasing the control to show that printing is busy
                await Task.Delay(TimeSpan.FromMilliseconds(_settings.PrintDurationMs*2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while printing twice");
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while printing twice"));
                }
            }
            finally
            {
                //always reset the control to its initial state
                _hardware.PrintTwiceControl.Unlock(lockId);
                _hardware.PrintControl.Unlock(singleLockId);
            }
		}

        private async void OnTriggerControlTriggered(object sender, TriggerControlEventArgs e)
		{
			_logger.LogInformation("Trigger control fired");

			//Release the trigger to prevent double sessions
			var lockId = _hardware.TriggerControl.Lock (true);
			_hardware.PrintControl.Release ();
			_hardware.PrintTwiceControl.Release ();

            try
            {
                try
                {
                    _sessionLock.Wait();
                    if (_currentSession != null)
                    {
                        _logger.LogDebug("Disposing previous session");
                        _currentSession.Dispose();
                        _currentSession = null;
                    }
                    //Wait before taking the first picture if configured
                    if (_settings.TriggerDelayMs > 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(_settings.TriggerDelayMs));
                    }
                    //Take pictures and add to the queue
                    _currentSession = await _service.Capture();

                    if (_currentSession != null && _currentSession.ResultImage != null)
                    {
                        //Save the session for later use (e.g. offloading) if configured
                        if (_settings.SaveSessions)
                        {
                            await _service.Save(_currentSession);
                        }
                        //Afer capturing we're ready for printing or for another shoot
                        _hardware.PrintControl.Arm();
                        _hardware.PrintTwiceControl.Arm();
                    }
                    else
                    {
                        if (ErrorOccurred != null)
                        {
                            _logger.LogError("Capturing did not lead to a successful session");
                            ErrorOccurred.Invoke(this,
                                new ErrorEventArgs("Capturing did not lead to a successful session"));
                        }
                    }

                }
                finally
                {
                    _sessionLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while capturing images");
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while capturing images"));
                }
            }
            finally
            {
                //Always re-arm the trigger after shooting
                _hardware.TriggerControl.Unlock(lockId);
            }
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
		        if (disposing)
		        {
		            // Clean up managed objects
		        }
                _currentSession = null;
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

		~PhotoBoothModel()
		{
			Dispose (false);
		}
		
		#endregion

    }
}
