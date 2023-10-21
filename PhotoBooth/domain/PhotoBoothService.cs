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
using com.prodg.photobooth.infrastructure.hardware;
using com.prodg.photobooth.infrastructure.serialization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

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
		public PictureAddedEventArgs(Image picture, int index, int sessionSize)
        {
            Picture = picture;
			Index = index;
			SessionSize = sessionSize;
        }

        public Image Picture { get; }
		public bool IsFinal { get { return Index >= SessionSize; } }
		public int Index { get; }
		public int SessionSize { get; }
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
        private readonly ILogger<PhotoBoothService> _logger;
        private readonly IHardware _hardware;
        private readonly IMultiImageProcessor _imageProcessor;
        private readonly PhotoSessionFactory _sessionFactory;
        private readonly IPhotoboothOffloader _offloader;

        public event EventHandler<PictureAddedEventArgs>? PictureAdded;

        /// <summary>
        /// C'tor
        /// </summary>
        public PhotoBoothService(IHardware hardware, IMultiImageProcessor imageProcessor,
	         ISettings settings, IPhotoboothOffloader offloader, ILogger<PhotoBoothService> logger)
        {
	        _hardware = hardware;
	        _logger = logger;
	        _imageProcessor = imageProcessor;
	        _offloader = offloader;

	        logger.LogDebug("Creating PhotoBooth Service");

	        _sessionFactory = new PhotoSessionFactory(settings);
        }

        /// <summary>
        /// Capture a photo session
        /// </summary>
        /// <returns>The session containing the captured and processed images</returns>
        public async Task<PhotoSession?> Capture()
		{
            var session = _sessionFactory.CreateSession();
            _logger.LogInformation("Start Capturing images for event {EventId}, session {Id}",
                session.EventId, session.Id);
                
			return await Task.Run (() => {
				try {
					int failureCount = 0;

					while (session.ImageCount < _imageProcessor.RequiredImages && failureCount < 10) {
						string imagePath = session.GetNextImagePath ();
						if (_hardware.Camera.Capture (imagePath)) {
							Image image = session.AddPicture (imagePath);
							//Signal that a new picture was added
							if (PictureAdded != null) {
								PictureAdded(this, new PictureAddedEventArgs (image, session.ImageCount - 1, 
									_imageProcessor.RequiredImages));
							}
						} else {
							failureCount++;
						}
					}
					session.ResultImage = _imageProcessor.Process (session);
					//Signal that a new picture was added
					if (PictureAdded != null) {
						PictureAdded(this, new PictureAddedEventArgs (session.ResultImage, session.ImageCount, 
							_imageProcessor.RequiredImages));
					}

					//Return the session
					return session;
				} catch (Exception ex) {
					_logger.LogError(ex, "Error while capturing images");
					return null;
				} finally {
					//Clean all images from the camera buffer
					_hardware.Camera.Clean ();
				}
			});
		}

        /// <summary>
        /// Print a photo session
        /// </summary>
        /// <param name="session"></param>
        public async Task Print(PhotoSession? session)
		{
			_logger.LogInformation("Start Printing result image for session {StoragePath}", session.StoragePath);

            await Task.Run(() =>
            {
                try
                {
	                _hardware.Printer.Print(session.ResultImage ??
	                                        throw new InvalidOperationException("Cannot print: ResultImage is null"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while printing image");
                }
            });
		}

        /// <summary>
        /// Save a photo session
        /// </summary>
        /// <param name="session"></param>
        public async Task Save(PhotoSession? session)
        {
            _logger.LogInformation("Start saving session {StoragePath}", session.StoragePath);

            await Task.Run(() =>
            {
                try
                {
                    _offloader.OffloadSession(session.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while saving session");
                }
            });
        }
    }
}
