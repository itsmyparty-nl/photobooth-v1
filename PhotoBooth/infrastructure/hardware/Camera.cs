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

using System.Text.RegularExpressions;
using LibGPhoto2;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class Camera : ICamera
	{
		private IContext _context;
		private LibGPhoto2.ICamera _camera;
		private readonly ILogger<Camera> _logger;
		private bool _initialized;
	    private bool _deinitRequested;
	    private Thread _monitoringThread;
	    private const string CameraBaseFolder = @"/";
	    private const int DefaultBatteryLevel = 99;
	    private const int WarningBatteryLevel = 25;

        private readonly object _cameraLock = new();

		public string Id { get; private set; }

		public Camera(ILogger<Camera> logger)
		{
            logger.LogDebug("Creating camera interface");
            _logger = logger;
			_initialized = false;
            _deinitRequested = false;
		}

		public void Initialize ()
		{
		    if (_initialized) return;

            _logger.LogInformation("Initializing camera");
		    _deinitRequested = false;
            _monitoringThread = new Thread(MonitorCameraAsync);
		    _monitoringThread.Start();
		}

	    private void MonitorCameraAsync()
	    {
	        int pollingIndex = 0;
	        while (!_deinitRequested)
	        {
	            lock (_cameraLock)
	            {
	                if (!_initialized)
	                {
	                    TryInitialize();
	                }
	                else
	                {
	                    pollingIndex++;
	                    //Check with a reduced frequency
	                    if (pollingIndex%5 != 0) continue;

	                    MonitorCameraPresence();
	                    pollingIndex = 0;
	                }
	            }
	            //Sleep until the next poll
                if (!_deinitRequested)
                {
                    Thread.Sleep(3000);
                }
	        }
	    }

	    private void MonitorCameraPresence()
	    {
		    try
	        {
		        int level;
		        lock (_cameraLock)
	            {
	                level = GetBatteryLevel();
	            }
	            //Notify users if the battery level is at a warning level
	            if (level <= WarningBatteryLevel)
	            {
		            BatteryWarning.Invoke(this, new CameraBatteryWarningEventArgs(level));
	            }
	        }
	        catch (Exception)
	        {
	            _logger.LogWarning("Connection with camera lost");
	            DisposeCameraObjects();
	            _initialized = false;

                //Signal that the camera is lost (not within the lock)
                StateChanged.Invoke(this, new CameraStateChangedEventArgs(false));
	        }
	    }

	    private void TryInitialize()
	    {
		    try
		    {
			    lock (_cameraLock)
			    {
				    //Initialize GPhoto2
				    _context = new Context();
				    _camera = new LibGPhoto2.Camera();
				    _camera.Init(_context);

				    //Get the ID of the camera
				    CameraAbilities abilities = _camera.GetAbilities();
				    Id = abilities.model;

				    //Log the ID
				    _logger.LogInformation("Found: {Id}", Id);
				    _logger.LogDebug("Status: {Status}", abilities.status);
				    _logger.LogDebug("Id: {Id}", abilities.id);

				    _initialized = true;
			    }

			    //Signal that the camera is ready (not within the lock)
			    StateChanged.Invoke(this, new CameraStateChangedEventArgs(true));
		    }
		    catch (Exception exception)
		    {
			    _logger.LogDebug(exception, "Could not initialize camera: {Message}", exception.Message);
		    }
	    }


	    /// <remarks>In all cases that no value can be retrieved a default battery level is returned which
	    /// indicates a full battery. rationale is that in this case manual checking is needed, and the
	    /// software should work without issues</remarks>
        private int GetBatteryLevel()
	    {
	        string summary;
            lock (_cameraLock)
            {
                if (!CheckInitialized()) return DefaultBatteryLevel;

	            summary = _camera.GetSummary(_context).Text;
	        }

	        using (var reader = new StringReader(summary))
	        {
	            string? line;
	            while ((line = reader.ReadLine()) != null)
	            {
	                if (!line.Contains("Battery")) continue;

	                Regex regex = new Regex(@"^.+value\: (?<level>\d+)\%.*");
	                Match match = regex.Match(line);

	                if (!match.Success) return DefaultBatteryLevel;

	                try
	                {
	                    return Convert.ToInt32(match.Groups["level"].Value);
	                }
	                catch (Exception ex)
	                {
	                    _logger.LogDebug(ex, "no match found for battery level");
	                    return DefaultBatteryLevel;
	                }
	            }
	        }
	        return DefaultBatteryLevel;
	    }

	    [Obsolete("Obsolete")]
	    public void DeInitialize()
	    {
            _logger.LogInformation("DeInitializing camera");
	        _deinitRequested = true;

	        if (!Thread.CurrentThread.Equals(_monitoringThread))
	        {
	            if (!_monitoringThread.Join(5000))
	            {
	                _logger.LogError("Cancel initialize failed, aborting thread");
                    _monitoringThread.Abort();
	            }
	        }

	        lock (_cameraLock)
	        {
		        DisposeCameraObjects();
	        }
	    }

	    public bool Capture(string capturePath)
	    {
	        _logger.LogDebug("Starting capture");
	        lock (_cameraLock)
	        {
	            if (!CheckInitialized())
	            {
	                return false;
	            }

	            try
	            {
	                //Always get the battery level to make sure the battery warning is signaled timely
	                _logger.LogDebug("Battery Level: {Level}", GetBatteryLevel());

	                //Capture and download
					ICameraFilePath path = _camera.Capture(CameraCaptureType.Image, _context);
	                _logger.LogDebug("Capture finished. File {File}", Path.Combine(path.folder, path.name));
	                ICameraFile cameraFile = _camera.GetFile(path.folder, path.name,
	                    CameraFileType.Normal,
	                    _context);

	                _logger.LogInformation("Saving file to {CapturePath}", capturePath);
	                cameraFile.Save(capturePath);

	                //Remove the file from the camera buffer
	                _camera.DeleteFile(path.folder, path.name, _context);
	                return true;
	            }
	            catch (Exception exception)
	            {
		            _logger.LogError(exception, "Camera capture failed");
	                return false;
	            }
	        }
	    }

	    public void Clean ()
		{
	        lock (_cameraLock)
	        {
                if (!CheckInitialized()) return;

	            //Try to delete all images on the camera
                _camera.DeleteAll(CameraBaseFolder, _context);

                //Deinitialize to fix slow reponse issues after multiple sessions
                DisposeCameraObjects();
	            _initialized = false;
                //note: the monitor thread will re-initialize the camera
	        }
		}

		private bool CheckInitialized ()
		{
			if (!_initialized) {
				_logger.LogError ("Camera not initialized");
				return false;
			}
			return true;
		}

		#region IDisposable Implementation

		bool _disposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (_disposed) return;
			if (disposing) {
				// Clean up managed objects
					
			}
			// clean up any unmanaged objects
			DisposeCameraObjects();
			_disposed = true;
		}

	    private void DisposeCameraObjects()
	    {
	        try
	        {
	            lock (_cameraLock)
	            {
		            try
	                {
		                _camera.Exit(_context);
	                }
	                catch (Exception)
	                {
		                _logger.LogWarning("Could not Exit camera from context");
	                }
	                _camera.Dispose();

	                _context.Dispose();
	            }
	        }
	        catch (Exception ex)
	        {
	            _logger.LogError(ex, "Exception while disposing camera objects");
	        }
	    }

	    ~Camera ()
		{
			Dispose (false);
		}
		
		#endregion


        #region ICamera Members

        public event EventHandler<CameraStateChangedEventArgs> StateChanged;

        public event EventHandler<CameraBatteryWarningEventArgs> BatteryWarning;

        #endregion
    }
}
