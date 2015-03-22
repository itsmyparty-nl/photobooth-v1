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
using System.Threading.Tasks;

namespace com.prodg.photobooth.domain
{
    public interface IPhotoBoothService
    {
        event EventHandler<PictureAddedEventArgs> PictureAdded;

        /// <summary>
        /// Capture a photo sessio
        /// </summary>
        /// <returns>The session containing the captured and processed images</returns>
        Task<PhotoSession> Capture();

        /// <summary>
        /// Print a photo session
        /// </summary>
        /// <param name="session"></param>
        Task Print(PhotoSession session);

        /// <summary>
        /// Saves a photo session to disk for later use 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task Save(PhotoSession session);
    }
}