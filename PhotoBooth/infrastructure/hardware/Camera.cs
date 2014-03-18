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
using System.IO;
using System.Threading;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class Camera : ICamera
	{
		private LibGPhoto2.IContext context;
		private LibGPhoto2.ICamera camera;
		private readonly ILogger logger;
		private bool initialized;

		public string Id { get; private set; }

		public Camera (ILogger logger)
		{
			this.logger = logger;
			initialized = false;
		}

		public bool Initialize ()
		{
			logger.LogInfo ("Initializing camera");
			try {
				//Initialize GPhoto2
				context = new LibGPhoto2.Context ();
				camera = new LibGPhoto2.Camera ();
				camera.Init (context);
				
				//Get the ID of the camera
				LibGPhoto2.CameraAbilities abilities = camera.GetAbilities ();
				Id = abilities.model;
				LibGPhoto2.ICameraWidget widget = camera.GetConfig (context);
				logger.LogInfo ("Children: " + widget.ChildCount);
				
				//logger.LogInfo ("Summary: " + camera.GetSummary(context).Text);
				//	LibGPhoto2.CameraWidget childWidget = widget.GetChild(0);
				//	logger.LogInfo (childWidget.GetInfo());
				
				//LibGPhoto2.CameraWidget battLevelWidget = widget.GetChild("batterylevel");
				//logger.LogInfo ("Battery: " + battLevelWidget.GetInfo());
				
				//Log the ID
				logger.LogInfo ("Found: " + Id);
				//System.Console.WriteLine ("about: "+ camera.GetAbout(context).Text);
				logger.LogInfo ("operations: " + abilities.operations.ToString ());
				
				initialized = true;
				return true;
				
			} catch (Exception exception) {
				logger.LogException ("Could not initialize camera", exception);
				return false;
			}
		}

	    public bool Capture(string capturePath)
	    {
	        logger.LogInfo("Starting capture");
	        if (!CheckInitialized())
	        {
	            return false;
	        }

	        try
	        {
	            LibGPhoto2.ICameraFilePath path = camera.Capture(LibGPhoto2.CameraCaptureType.Image, context);
	            logger.LogInfo("Capture finished. File: " + path.folder + "\\" + path.name);
	            LibGPhoto2.ICameraFile cameraFile = camera.GetFile(path.folder, path.name, LibGPhoto2.CameraFileType.Normal,
	                                                               context);

                logger.LogInfo("Saving file to: "+capturePath);
                cameraFile.Save(capturePath);

                //Remove the file from the camera buffer
                camera.DeleteFile(path.folder, path.name, context);
	            return true;
	        }
	        catch (Exception exception)
	        {
	            logger.LogException("Camera capture failed, starting camera re-init", exception);
	            ResetCameraOnFailure();
	            return false;
	        }
	    }

	    public void Clean ()
		{
			camera.DeleteAll ("/", context);
		}

		private bool CheckInitialized ()
		{
			if (!initialized) {
				logger.LogError ("Camera not initialized");
				return false;
			}
			return true;
		}

		private void ResetCameraOnFailure ()
		{
			if (!CheckInitialized ())
				return;
			logger.LogInfo ("Releasing camera");
			
			DisposeCameraObjects ();
			
			Thread.Sleep (500);
			//ReInitialize after releasing camera
			Initialize ();
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
				DisposeCameraObjects ();
				disposed = true;
			} else {
				Console.WriteLine ("Saved us from doubly disposing an object!");
			}
		}

		private void DisposeCameraObjects ()
		{
			try {
				if (context != null) {
					if (camera != null) {
						try {
							camera.Exit (context);
						} catch (Exception) {
							logger.LogWarning ("Could not Exit camera from context");
						} finally {
							camera.Dispose ();
							
						}
					}
					context.Dispose ();
				}
			} catch (Exception) {
				//Do nothing
			} finally {
				context = null;
				camera = null;
			}
		}

		~Camera ()
		{
			Dispose (false);
		}
		
		#endregion
		
	}
}
