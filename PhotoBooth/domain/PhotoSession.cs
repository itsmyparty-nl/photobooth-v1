/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

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
        }

        public string Finish()
        {
            try
            {
                return imageProcessor.Process(Id, pictures);
            }
            catch (Exception ex)
            {
                logger.LogException("Error finishing photo session", ex);
                return null;
            }
        }
    }
}

