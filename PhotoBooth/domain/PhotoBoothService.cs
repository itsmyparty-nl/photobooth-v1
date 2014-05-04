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
using System.Globalization;
using System.Threading.Tasks;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.domain
{
    #region Picture Added eventargs
    
    /// <summary>
    /// Event args to use for signalling that an image was added
    /// </summary>
    /// <remarks>Images are passed as references in the event args. These should not be stored or kept since this is
    /// they can be disposed at will by the model</remarks>
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

    #endregion

    /// <summary>
    /// The Photobooth service.
    /// <para>
    /// The service is responsible for implementing the logic to perform photobooth tasks like printing and capturing.
    /// It groups and forwards calls to infrastructure classes and stores information in domain entities. The service
    /// is also responsible to signaling that pictures are taken so that they can be 
    /// </para>
    /// </summary>
    public class PhotoBoothService : IPhotoBoothService
    {
        private readonly ILogger logger;
        private readonly IHardware hardware;
        private readonly IImageProcessor imageProcessor;
        private readonly ISettings settings;

        public event EventHandler<PictureAddedEventArgs> PictureAdded;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="hardware"></param>
        /// <param name="imageProcessor"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PhotoBoothService(IHardware hardware, IImageProcessor imageProcessor, ILogger logger, ISettings settings)
        {
            this.hardware = hardware;
            this.logger = logger;
            this.settings = settings;
            this.imageProcessor = imageProcessor;
        }

        /// <summary>
        /// Capture a photo session
        /// </summary>
        /// <returns>The session containing the captured and processed images</returns>
        public async Task<PhotoSession> Capture()
        {
            logger.LogInfo("Start Capturing images");
            try
            {
                int failureCount = 0;
                var session = new PhotoSession(settings);

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
                session.ResultImage = imageProcessor.Process(session);
                //Signal that a new picture was added
                if (PictureAdded != null)
                {
					PictureAdded.Invoke(this, new PictureAddedEventArgs(session.ResultImage, false));
                }
                //Return the session
                return session;
            }
			catch (Exception ex) {
				logger.LogException ("Error while capturing images", ex);
				return null;
			}
            finally
            {
                //Clean all images from the camera buffer
                hardware.Camera.Clean();
            }
        }

        /// <summary>
        /// Print a photo session
        /// </summary>
        /// <param name="session"></param>
        public void Print(PhotoSession session)
        {
            logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Start Printing images for session {0}", session.StoragePath));
            
            //Printing to be implemented
        }
    }
}
