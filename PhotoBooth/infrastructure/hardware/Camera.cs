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
using System.Text.RegularExpressions;
using System.Threading;
using com.prodg.photobooth.common;
using LibGPhoto2;
using NLog;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class Camera : ICamera
	{
		private LibGPhoto2.IContext context;
		private LibGPhoto2.ICamera camera;
		private readonly ILogger logger;
		private bool initialized;
	    private bool deinitRequested;
	    private Thread initializeThread;
	    private const string CameraBaseFolder = @"/";
	    private const int DefaultBatteryLevel = 99;
	    private const int WarningBatteryLevel = 25;


		public string Id { get; private set; }

		public Camera (ILogger logger)
		{
			this.logger = logger;
			initialized = false;
            deinitRequested = false;
		}

		public void Initialize ()
		{
		    if (initialized) return;

            logger.LogInfo ("Initializing camera");
		    deinitRequested = false;
            initializeThread = new Thread(InitializeCameraAsync);
		    initializeThread.Start();
		}

	    private void InitializeCameraAsync()
	    {
	        while (!initialized && !deinitRequested)
	        {
	            Thread.Sleep(500);
                try
	            {
	                //Initialize GPhoto2
	                context = new LibGPhoto2.Context();
	                camera = new LibGPhoto2.Camera();
	                camera.Init(context);

	                //Get the ID of the camera
	                LibGPhoto2.CameraAbilities abilities = camera.GetAbilities();
	                Id = abilities.model;

	                //Log the ID
	                logger.LogInfo("Found: " + Id);
	                logger.LogInfo("Battery Level: " + GetBatteryLevel());
	                logger.LogDebug("Status: " + abilities.status);
	                logger.LogDebug("Id: " + abilities.id);

	                initialized = true;

	                //Signal that the camera is ready
                    if (Ready != null)
	                {
	                    Ready.Invoke(this, EventArgs.Empty);
	                }
	            }
	            catch (Exception exception)
	            {
	                logger.LogDebug("Could not initialize camera: " + exception.Message);
	            }
	        }
	    }

	    /// <remarks>In all cases that no value can be retrieved a default battery level is returned which
	    /// indicates a full battery. rationale is that in this case manual checking is needed, and the
	    /// software should work without issues</remarks>
        private int GetBatteryLevel()
	    {
	        var summary = camera.GetSummary(context).Text;
	        using (var reader = new StringReader(summary))
	        {
	            string line;
	            while ((line = reader.ReadLine()) != null)
	            {
	                if (!line.Contains("Battery")) continue;

	                Regex regex = new Regex(@"^.+value\: (?<level>\d+)\%.*");
	                Match match = regex.Match(line);

	                if (!match.Success) return DefaultBatteryLevel;

	                try
	                {
	                    var level = Convert.ToInt32(match.Groups["level"].Value);
	                    if (level <= WarningBatteryLevel)
	                    {
	                        if (BatteryWarning != null)
	                        {
	                            BatteryWarning.Invoke(this, new CameraBatteryWarningEventArgs(level));
	                        }
	                    }
	                }
	                catch (Exception ex)
	                {
	                    logger.LogDebug("no match found for battery level: " + ex.Message);
	                    return DefaultBatteryLevel;
	                }
	            }
	        }
	        return DefaultBatteryLevel;
	    }

	    public void DeInitialize()
	    {
            logger.LogInfo("DeInitializing camera");
	        deinitRequested = true;

	        if (initializeThread != null && !Thread.CurrentThread.Equals(initializeThread))
	        {
	            if (!initializeThread.Join(5000))
	            {
	                logger.LogError("Cancel initialize failed, aborting thread");
                    initializeThread.Abort();
	            }
	        }

	        if (context == null) return;
	        if (camera == null) return;
	        
            try
	        {
	            camera.Exit(context);
	        }
	        catch (Exception)
	        {
	            logger.LogWarning("Could not Exit camera from context");
	        }

            DisposeCameraObjects();
	    }

	    public bool Capture(string capturePath)
	    {
	        logger.LogDebug("Starting capture");
	        if (!CheckInitialized())
	        {
	            return false;
	        }

	        try
	        {
	            //Always get the battery level to make sure the battery warning is signaled timely
                logger.LogDebug("Battery Level: "+GetBatteryLevel());

	            //Capture and download
                LibGPhoto2.ICameraFilePath path = camera.Capture(LibGPhoto2.CameraCaptureType.Image, context);
	            logger.LogDebug("Capture finished. File: " + Path.Combine(path.folder, path.name));
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
            camera.DeleteAll(CameraBaseFolder, context);
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
			logger.LogInfo ("Releasing camera");
			
			DeInitialize();
			
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
				DisposeCameraObjects();
				disposed = true;
			}
		}

	    private void DisposeCameraObjects()
	    {
	        try
	        {
	            if (camera != null)
	            {
	                camera.Dispose();
	            }
	            if (context != null)
	            {
	                context.Dispose();
	            }
	        }
	        catch (Exception ex)
	        {
	            logger.LogException("Exception while disposing camera objects", ex);
	        }
	        finally
	        {
	            context = null;
	            camera = null;
	        }
	    }

	    ~Camera ()
		{
			Dispose (false);
		}
		
		#endregion


        #region ICamera Members

        public event EventHandler Ready;

        public event EventHandler<CameraBatteryWarningEventArgs> BatteryWarning;

        #endregion
    }
}
