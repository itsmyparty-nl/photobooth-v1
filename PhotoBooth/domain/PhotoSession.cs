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
using System.Globalization;
using System.IO;
using com.prodg.photobooth.config;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoSession: IDisposable
    {
        /// <summary>
        /// Cache the last index for performance, consider an alternate implementation
        /// </summary>
        private static int _lastSessionIndex;  
        
        private readonly ISettings settings;
        
        public string StoragePath { get; private set; }
        
        public List<Image> Images { get; private set; }

        public Image ResultImage { get; set; }

        public int ImageCount { get { return Images.Count; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PhotoSession(ISettings settings)
        {
            Images = new List<Image>();
            this.settings = settings;

            PrepareSession();
        }

        public string GetNextImagePath()
        {
            return Path.Combine(StoragePath, (ImageCount +1) + ".jpg");
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

        #region Helper methods
        
        private string PrepareSession()
        {
            int sessionIndex = GetNextSessionIndex();
            string storagePath = GetSessionPath(sessionIndex);
            
            //Note: do not catch the exceptions since this is inrecoverable
            Directory.CreateDirectory(storagePath);

            return storagePath;
        }

        private int GetNextSessionIndex()
        {
            _lastSessionIndex++;
            //Get the next session storage path which is unused
            while (Directory.Exists(GetSessionPath(_lastSessionIndex)))
            {
                _lastSessionIndex++;
            }
            return _lastSessionIndex;
        }

        private string GetSessionPath(int index)
        {
            return Path.Combine(settings.StoragePath, index.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

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

