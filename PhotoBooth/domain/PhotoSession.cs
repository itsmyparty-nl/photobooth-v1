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
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PhotoSession: IDisposable
    {
        [JsonProperty]
        public string EventId { get; private set; }

        [JsonProperty]
        public string Id { get; private set; }

        [JsonProperty]
        public string StoragePath { get; private set; }
        
        [JsonProperty(ItemConverterType = typeof(ImageConverter))]
        public List<Image> Images { get; private set; }

        [JsonProperty(ItemConverterType = typeof(ImageConverter))]
        public Image ResultImage { get; set; }

        public int ImageCount { get { return Images.Count; } }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        [JsonConstructor]
        private PhotoSession()
        {
            Images = new List<Image>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="id"></param>
        /// <param name="storagePath"></param>
        public PhotoSession(string eventId, string id, string storagePath)
            : this()
        {
            EventId = eventId;
            Id = id;
            StoragePath = storagePath;
        }

        public string GetNextImagePath()
        {
			return Path.Combine(StoragePath, String.Format("{0}.jpg",ImageCount +1));
        }

        public Image AddPicture(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Cannot resolve an image from an empty path");
            }
            Image image = Image.FromFile(path);
            Images.Add(image);

            return image;
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
		            if (Images != null)
		            {
		                foreach (var image in Images)
		                {
		                    image.Dispose();
		                }
		                Images.Clear();
		                Images = null;
		            }

		            if (ResultImage != null)
		            {
		                ResultImage.Dispose();
		                ResultImage = null;
		            }
		        }
		        // clean up any unmanaged objects
		        disposed = true;
		    }
		}

		~PhotoSession()
		{
			Dispose (false);
		}
		
		#endregion
    }
}

