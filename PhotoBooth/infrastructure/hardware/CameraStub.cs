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
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class CameraStub : ICamera
	{
		private readonly ILogger logger;

	    public event EventHandler Ready;
	    public event EventHandler<CameraBatteryWarningEventArgs> BatteryWarning;
	    public string Id { get; private set; }

		public CameraStub (ILogger logger)
		{
			this.logger = logger;
		    Id = "Dummy";
		}

		public void Initialize ()
		{
			logger.LogInfo ("Initializing DUMMY camera");
		}

	    public void DeInitialize()
	    {
            logger.LogInfo("DeInitializing DUMMY camera");
	    }

	    public bool Capture(string capturePath)
	    {
	        logger.LogInfo("Starting capture to: "+capturePath);
	        return true;
	    }

	    public void Clean ()
		{
	        logger.LogInfo("Cleaning camera");
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
			if (!disposed) {
				if (disposing) {
					// Clean up managed objects
					
				}
				// clean up any unmanaged objects
				disposed = true;
			}
		}

		~CameraStub ()
		{
			Dispose (false);
		}
		
		#endregion
		
	}
}
