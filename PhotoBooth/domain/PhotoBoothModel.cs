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
using System.Threading.Tasks;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
	/// <summary>
    /// The Photobooth application model.
    /// <see cref="IPhotoBoothModel"/>
    /// </summary>
    public class PhotoBoothModel : IPhotoBoothModel
    {
        private readonly ILogger logger;
        private readonly IHardware hardware;
        private readonly IPhotoBoothService service;
        private readonly SemaphoreSlim sessionLock;
		private PhotoSession currentSession;

        /// <summary>
        /// Event to signal that shutdown is requested
        /// </summary>
        /// <remarks>This event is added to provide a single location for handling all hardware controls, while keeping
        /// the responsibility of stopping the model at the application class</remarks>
        public event EventHandler ShutdownRequested;

		/// <summary>
		/// Occurs when error occurred.
		/// </summary>
		public event EventHandler<ErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hardware"></param>
        /// <param name="logger"></param>
        public PhotoBoothModel(IPhotoBoothService service, IHardware hardware, ILogger logger)
        {
            this.hardware = hardware;
            this.logger = logger;
            this.service = service;
			sessionLock = new SemaphoreSlim (1);

            currentSession = null;
        }

        /// <summary>
        /// Start the photobooth model
        /// </summary>
        public void Start()
        {
            logger.LogInfo("Starting Photobooth application model");

            //Acquire the hardware
            hardware.Acquire();

            //Start with only the power control armed
            hardware.PowerControl.Arm();
   
            //Register events
            hardware.Camera.StateChanged += OnCameraStateChanged;
            hardware.TriggerControl.Fired += OnTriggerControlTriggered;
            hardware.PrintControl.Fired += OnPrintControlTriggered;
            hardware.PrintTwiceControl.Fired += OnPrintTwiceControlTriggered;
            hardware.PowerControl.Fired += OnPowerControlTriggered;
        }

        /// <summary>
        /// Stop the photobooth model
        /// </summary>
        public void Stop()
        {
            logger.LogInfo("Stopping Photobooth application model");

            //Unsubscribe from all hardware events
            hardware.Camera.StateChanged -= OnCameraStateChanged;
            hardware.TriggerControl.Fired -= OnTriggerControlTriggered;
            hardware.PrintControl.Fired -= OnPrintControlTriggered;
            hardware.PrintTwiceControl.Fired -= OnPrintTwiceControlTriggered;
            hardware.PowerControl.Fired -= OnPowerControlTriggered;

            //Release the hardware
            hardware.Release();
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
                Task.Run(() => hardware.TriggerControl.Arm());
            }
            else
            {
                Task.Run(() => hardware.TriggerControl.Release());
            }
        }

        private void OnPowerControlTriggered(object sender, TriggerControlEventArgs e)
        {
            logger.LogInfo("Power control fired");

            try
            {
                //Trigger a shutdown request.
                //Do not call stop directly since this is the responsibility of the application
                ShutdownRequested.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                //Log the exception
                logger.LogException("Error while handling shutdown", ex);
                //Rethrow since no mitigation is possible
                throw;
            }
        }

        private async void OnPrintControlTriggered(object sender, TriggerControlEventArgs e)
		{
			logger.LogInfo ("Print control fired");
           
			//lock both buttons but only indicate that the first is printing
			var lockId = hardware.PrintControl.Lock (true);
            var twiceLockId = hardware.PrintTwiceControl.Lock(false);
            try
            {
                try
                {
                    sessionLock.Wait();
                    //Print
                    if (currentSession != null)
                    {
                        //Get the last sesion from the queue and print it
                        await service.Print(currentSession);
                    }
                    else
                    {
                        logger.LogWarning("Nothing to print");
                    }
                }
                finally
                {
                    sessionLock.Release();
                }

                //Wait until releasing the control to show that printing is busy
                await Task.Delay(TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                logger.LogException("Error while printing", ex);
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while printing"));
                }
            }
            finally
            {
                //always reset the control to its initial state
                hardware.PrintControl.Unlock(lockId);
                hardware.PrintTwiceControl.Unlock(twiceLockId);
            }
		}

        private async void OnPrintTwiceControlTriggered(object sender, TriggerControlEventArgs e)
		{
			logger.LogInfo ("Print twice control fired");

			//Release the print button to prevent printing twice
<<<<<<< HEAD
			//hardware.PrintControl.Release ();
=======
>>>>>>> f8d467da70598d2c9c4d221cff43c2dbb28ce383
			var lockId = hardware.PrintTwiceControl.Lock (true);
            var singleLockId = hardware.PrintControl.Lock(false);

            try
            {
                try
                {

                    sessionLock.Wait();
                    if (currentSession != null)
                    {
                        //Get the last sesion from the queue and print it
                        await service.Print(currentSession);
                        await service.Print(currentSession);
                    }
                    else
                    {
                        logger.LogInfo("Nothing to print");
                        return;
                    }

                }
                finally
                {
                    sessionLock.Release();
                }
                //Wait until releasing the control to show that printing is busy
                await Task.Delay(TimeSpan.FromSeconds(90));
            }
            catch (Exception ex)
            {
                logger.LogException("Error while printing twice", ex);
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while printing twice"));
                }
            }
            finally
            {
                //always reset the control to its initial state
                hardware.PrintTwiceControl.Unlock(lockId);
                hardware.PrintControl.Unlock(singleLockId);
            }
		}

        private async void OnTriggerControlTriggered(object sender, TriggerControlEventArgs e)
		{
			logger.LogInfo ("Trigger control fired");

			//Release the trigger to prevent double sessions
			var lockId = hardware.TriggerControl.Lock (true);
			hardware.PrintControl.Release ();
			hardware.PrintTwiceControl.Release ();

            try
            {
                try
                {
                    sessionLock.Wait();
                    if (currentSession != null)
                    {
                        logger.LogDebug("Disposing previous session");
                        currentSession.Dispose();
                        currentSession = null;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    //Take pictures and add to the queue
                    currentSession = await service.Capture();

                    if (currentSession != null && currentSession.ResultImage != null)
                    {
                        //Afer capturing we're ready for printing or for another shoot
                        hardware.PrintControl.Arm();
                        hardware.PrintTwiceControl.Arm();
                    }
                    else
                    {
                        if (ErrorOccurred != null)
                        {
                            logger.LogError("Capturing did not lead to a successful session");
                            ErrorOccurred.Invoke(this,
                                new ErrorEventArgs("Capturing did not lead to a successful session"));
                        }
                    }

                }
                finally
                {
                    sessionLock.Release();
                }
            }
            catch (Exception ex)
            {
                logger.LogException("Error while capturing images", ex);
                if (ErrorOccurred != null)
                {
                    ErrorOccurred.Invoke(this, new ErrorEventArgs("Error while capturing images"));
                }
            }
            finally
            {
                //Always re-arm the trigger after shooting
                hardware.TriggerControl.Unlock(lockId);
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
                currentSession = null;
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
