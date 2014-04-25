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

namespace com.prodg.photobooth.domain
{
    public class PhotoSession : IPhotoSession
    {
        private readonly List<string> pictures;
        private readonly IImageProcessor imageProcessor;
        private readonly ILogger logger;

		public event EventHandler<PictureAddedEventArgs> PictureAdded;
		public event EventHandler<PictureAddedEventArgs> Finished;

        public string Id { get; private set; }

        public string StoragePath { get; private set; }

        public PhotoSession(string id, string storagePath, IImageProcessor imageProcessor, ILogger logger)
        {
            Id = id;
            StoragePath = storagePath;

            pictures = new List<string>();
            this.imageProcessor = imageProcessor;
            this.logger = logger;
        }

        public void AddPicture(string path)
        {
            pictures.Add(path);
			//Send out an event that a picture was added
			PictureAdded.Invoke(this, new PictureAddedEventArgs(path, pictures.Count));
        }

        public string Finish()
        {
            try
            {
				string imagePath = imageProcessor.Process(Id, StoragePath, pictures);
				Finished.Invoke(this, new PictureAddedEventArgs(imagePath, pictures.Count));
				return imagePath;
            }
            catch (Exception ex)
            {
                logger.LogException("Error finishing photo session", ex);
                return null;
            }
        }
    }
}

