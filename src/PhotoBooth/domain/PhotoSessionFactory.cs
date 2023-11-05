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

using System.Globalization;
using System.IO;
using com.prodg.photobooth.config;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoSessionFactory
    {
        /// <summary>
        /// Cache the last index for performance, consider an alternate implementation
        /// </summary>
        private static int _lastSessionIndex;  
        
        private readonly ISettings settings;
                
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        public PhotoSessionFactory(ISettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Pick a path and create a new session for this path
        /// </summary>
        /// <returns>The new photosession</returns>
        public PhotoSession? CreateSession()
        {
            int sessionIndex = GetNextSessionIndex();
            string storagePath = GetSessionPath(sessionIndex);

            //Note: do not catch the exceptions since this is irrecoverable
            Directory.CreateDirectory(storagePath);

            return new PhotoSession(settings.EventId, sessionIndex, storagePath);
        }

        #region Helper methods
        
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
            return Path.Combine(settings.StoragePath, settings.EventId, index.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}

