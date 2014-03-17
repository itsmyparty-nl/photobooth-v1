/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
    public class PhotoBooth
    {
        private readonly ILogger logger;
        private readonly IHardware hardware;
        private readonly IImageProcessor imageProcessor;
        private readonly string baseStoragePath;

        private int sessionIndex;
        
        public readonly ManualResetEvent Finished = new ManualResetEvent(false);

        public PhotoBooth(IHardware hardware, ILogger logger)
        {
            this.hardware = hardware;
            this.logger = logger;
            imageProcessor = new ImageProcessor(logger);

            baseStoragePath = "/mnt/pictures/";
        }

        public void Start()
        {
            logger.LogInfo("Initializing Photobooth");
 
            hardware.Initialize();

            hardware.TriggerControl.Triggered += OnTriggerControlTriggered;
            hardware.PrintControl.Triggered += OnPrintControlTriggered;
            hardware.PowerControl.Triggered += OnPowerControlTriggered;

            //Start with the trigger control and power control prepared
            hardware.TriggerControl.Prepare();
            hardware.PowerControl.Prepare();
        }

        public void Stop()
        {
            logger.LogInfo("DeInitializing Photobooth");

            hardware.Release();

            hardware.TriggerControl.Triggered += OnTriggerControlTriggered;
            hardware.PrintControl.Triggered += OnPrintControlTriggered;
            hardware.PowerControl.Triggered += OnPowerControlTriggered;
        }

        private void OnPowerControlTriggered(object sender, RemoteControlEventArgs e)
        {
            Finished.Set();
        }

        private void OnPrintControlTriggered(object sender, RemoteControlEventArgs e)
        {
            hardware.PrintControl.Release();
            hardware.TriggerControl.Prepare();
        }

        private void OnTriggerControlTriggered(object sender, RemoteControlEventArgs e)
        {
            hardware.TriggerControl.Release();
            try
            {
                sessionIndex++;

                string sessionId = sessionIndex.ToString(CultureInfo.InvariantCulture);
                string storagePath = Path.Combine(baseStoragePath, sessionId);
                var session = new PhotoSession(sessionId, storagePath, imageProcessor, logger);
                
                int photoIndex = 0;
                int tryIndex = 0;
                while (photoIndex < 3 && tryIndex < 20)
                {
                    string imagePath = hardware.Camera.Capture(session.StoragePath);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        session.AddPicture(imagePath);
                        photoIndex++;
                    }
                    tryIndex++;
                }
                hardware.Camera.Clean();
                session.Finish();

                //Afer capturing we're ready for printing
                hardware.PrintControl.Prepare();
            }
            catch (Exception ex)
            {
                logger.LogException("Error while capturing images", ex);
                //In case anything went wrong, forget about this session and move on to the next
                hardware.TriggerControl.Prepare();
            }
        }
    }
}
