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
using System.Collections.Generic;
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
        private Queue<PhotoSession> sessionQueue; 

        /// <summary>
        /// Event to signal that shutdown is requested
        /// </summary>
        /// <remarks>This event is added to provide a single location for handling all hardware controls, while keeping
        /// the responsibility of stopping the model at the application class</remarks>
        public event EventHandler ShutdownRequested;

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

            sessionQueue = new Queue<PhotoSession>();
        }

        /// <summary>
        /// Start the photobooth model
        /// </summary>
        public void Start()
        {
            logger.LogInfo("Starting Photobooth application model");

            //Acquire the hardware
            hardware.Acquire();

            //
            hardware.TriggerControl.Fired += OnTriggerControlTriggered;
            hardware.PrintControl.Fired += OnPrintControlTriggered;
            hardware.PowerControl.Fired += OnPowerControlTriggered;

            //Start with the trigger control and power control armed
            hardware.TriggerControl.ArmTrigger();
            hardware.PowerControl.ArmTrigger();
            hardware.PrintControl.ReleaseTrigger();
        }

        /// <summary>
        /// Stop the photobooth model
        /// </summary>
        public void Stop()
        {
            logger.LogInfo("Stopping Photobooth application model");

            //Release all controls 
            hardware.TriggerControl.ReleaseTrigger();
            hardware.PowerControl.ReleaseTrigger();
            hardware.PrintControl.ReleaseTrigger();

            //Unsubscribe from all hardware events
            hardware.TriggerControl.Fired += OnTriggerControlTriggered;
            hardware.PrintControl.Fired += OnPrintControlTriggered;
            hardware.PowerControl.Fired += OnPowerControlTriggered;

            //Release the hardware
            hardware.Release();
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
                logger.LogException("Error while capturing images", ex);
                //Rethrow since no mitigation is possible
                throw;
            }
        }

        private void OnPrintControlTriggered(object sender, TriggerControlEventArgs e)
        {
            logger.LogInfo("Print control fired");
           
            try
            {
                //Release the print button to prevent printing twice
                hardware.PrintControl.ReleaseTrigger();
                //Print
                if (sessionQueue.Count > 0)
                {
                    //Get the last sesion from the queue and print it
                    var session = sessionQueue.Dequeue();
                    service.Print(session);
                    
                    //Dispose the session after printing to release the memory
                    session.Dispose();
                }
                else
                {
                    logger.LogInfo("Print control fired");
                    return;
                }
                //After printing we're ready for the next session
                hardware.TriggerControl.ArmTrigger();
            }
            catch (Exception ex)
            {
                logger.LogException("Error while capturing images", ex);
                //In case anything went wrong, there's most probably no use in trying again.
                //Forget about this session and move on to the next
                hardware.TriggerControl.ArmTrigger();
            }
        }

        private void OnTriggerControlTriggered(object sender, TriggerControlEventArgs e)
        {
            logger.LogInfo("Trigger control fired");
  
            try
            {
                //Release the trigger to prevent double sessions
                hardware.TriggerControl.ReleaseTrigger();
                //Take pictures and add to the queue
                sessionQueue.Enqueue(service.Capture());
                //Afer capturing we're ready for printing
                hardware.PrintControl.ArmTrigger();
            }
            catch (Exception ex)
            {
                logger.LogException("Error while capturing images", ex);
                //In case anything went wrong, forget about this session and move on to the next
                hardware.TriggerControl.ArmTrigger();
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
		            if (sessionQueue != null)
		            {
		                while (sessionQueue.Count > 0)
		                {
		                    sessionQueue.Dequeue().Dispose();
		                }
		                sessionQueue = null;
		            }
		        }
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
