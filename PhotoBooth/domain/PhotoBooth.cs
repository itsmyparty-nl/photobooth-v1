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
using System.Drawing;
using System.IO;
using System.Threading;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
    public class PictureAddedEventArgs : EventArgs
    {
        public PictureAddedEventArgs(Image picture, bool isFinal)
        {
            Picture = picture;
            IsFinal = isFinal;
        }

        public Image Picture { get; private set; }
        public bool IsFinal { get; private set; }
    }

    public class PhotoBooth: IDisposable
    {
        private readonly ILogger logger;
        private readonly IHardware hardware;
        private readonly IImageProcessor imageProcessor;
        private readonly ISettings settings;

        public event EventHandler<PictureAddedEventArgs> PictureAdded;

        public readonly ManualResetEvent Finished = new ManualResetEvent(false);

        public PhotoBooth(IHardware hardware, IImageProcessor imageProcessor, ILogger logger, ISettings settings)
        {
            this.hardware = hardware;
            this.logger = logger;
            this.settings = settings;
            this.imageProcessor = imageProcessor;
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
                int failureCount = 0;
                using (var session = new PhotoSession(settings))
                {
                    while (session.ImageCount < imageProcessor.RequiredImages && failureCount < 3)
                    {
                        string imagePath = session.GetNextImagePath();
                        if (hardware.Camera.Capture(imagePath))
                        {
                            Image image = session.AddPicture(imagePath);
                            //Signal that a new picture was added
                            if (PictureAdded != null)
                            {
                                PictureAdded.Invoke(this, new PictureAddedEventArgs(image, false));
                            }
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    Image finalImage = imageProcessor.Process(session);
                    //Signal that a new picture was added
                    if (PictureAdded != null)
                    {
                        PictureAdded.Invoke(this, new PictureAddedEventArgs(finalImage, false));
                    }
                }
                //Afer capturing we're ready for printing
                hardware.PrintControl.Prepare();
            }
            catch (Exception ex)
            {
                logger.LogException("Error while capturing images", ex);
                //In case anything went wrong, forget about this session and move on to the next
                hardware.TriggerControl.Prepare();
            }
            finally
            {
                hardware.Camera.Clean();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
