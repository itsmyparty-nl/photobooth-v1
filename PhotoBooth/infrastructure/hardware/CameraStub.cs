/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
using System.IO;
using System.Threading;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class CameraStub : ICamera
	{
		private readonly ILogger logger;

		public string Id { get; private set; }

		public CameraStub (ILogger logger)
		{
			this.logger = logger;
		    Id = "Dummy";
		}

		public bool Initialize ()
		{
			logger.LogInfo ("Initializing camera");
			try
			{
			    logger.LogInfo("Found: Dummy Camera");
				return true;
				
			} catch (Exception exception) {
				logger.LogException ("Could not initialize camera", exception);
				return false;
			}
		}

	    public string Capture(string capturePath)
	    {
	        logger.LogInfo("Starting capture");
	
	        string imagePath = Path.Combine(capturePath, "dummy.jpg");
	        return imagePath;
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
			} else {
				Console.WriteLine ("Saved us from doubly disposing an object!");
			}
		}

		~CameraStub ()
		{
			Dispose (false);
		}
		
		#endregion
		
	}
}
