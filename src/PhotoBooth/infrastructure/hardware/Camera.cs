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

using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class Camera : ICamera, IDisposable
	{
		private readonly ILogger<Camera> _logger;
		private readonly ICameraProvider _cameraHardware;
		private const int WarningBatteryLevel = 25;

		private bool _deinitRequested;
	    private Thread? _monitoringThread;

        private readonly object _cameraLock = new();

        public string Id => CheckInitialized() ? _cameraHardware!.Id : "Uninitialized";

		public bool IsReady => CheckInitialized() && !_deinitRequested;

		public Camera(ILogger<Camera> logger, ICameraProvider cameraHardware)
		{
            logger.LogDebug("Creating camera interface");
            _logger = logger;
			_cameraHardware = cameraHardware;
            _deinitRequested = false;
		}

		public void Initialize ()
		{
		    if (_cameraHardware.Initialized) {return;}

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
	                if (!_cameraHardware.Initialized)
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
		            BatteryWarning?.Invoke(this, new CameraBatteryWarningEventArgs(level));
	            }
	        }
	        catch (Exception)
	        {
	            _logger.LogWarning("Connection with camera lost");
	            _cameraHardware.Clean();

                //Signal that the camera is lost (not within the lock)
                StateChanged?.Invoke(this, new CameraStateChangedEventArgs(false));
	        }
	    }

	    private void TryInitialize()
	    {
		    try
		    {
			    lock (_cameraLock)
			    {
				    _cameraHardware.Initialize();

				    if (_cameraHardware.Info != null)
				    {
					    //Log the ID
					    _logger.LogInformation("Found: {Id}", _cameraHardware.Info.Model);
					    _logger.LogDebug("Status: {Status}", _cameraHardware.Info.Status);
					    _logger.LogDebug("Id: {Id}", _cameraHardware.Info.Id);
				    }
				    else
				    {
					    _logger.LogWarning("Camera Hardware info not set after initialization");
				    }
			    }

			    //Signal that the camera is ready (not within the lock)
			    StateChanged?.Invoke(this, new CameraStateChangedEventArgs(true));
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
            lock (_cameraLock)
            {
                if (!CheckInitialized()) return WarningBatteryLevel;

	            return _cameraHardware.GetBatteryLevel();
	        }
	    }

	    public void DeInitialize()
	    {
            _logger.LogInformation("DeInitializing camera");
	        _deinitRequested = true;

	        if (_monitoringThread != null && !Thread.CurrentThread.Equals(_monitoringThread) &&
	            !_monitoringThread.Join(5000))
	        {
		        _logger.LogError("Cancel initialize failed, aborting thread");
		        _monitoringThread.Abort();
	        }

	        lock (_cameraLock)
	        {
		        _cameraHardware.Clean();
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

	            //Always get the battery level to make sure the battery warning is signaled timely
	            _logger.LogDebug("Battery Level: {Level}", GetBatteryLevel());

	            return _cameraHardware.Capture(capturePath);
			}
	    }

	    public void Clean ()
		{
	        lock (_cameraLock)
	        {
                if (!CheckInitialized()) return;

	            _cameraHardware.Clean();
	        }
		}

		private bool CheckInitialized ()
		{
			if (!_cameraHardware.Initialized) {
				_logger.LogWarning ("Camera not initialized");
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
			if (disposing)
			{
				//add remove of managed objects here
			};
			_disposed = true;
		}

	    ~Camera ()
		{
			Dispose (false);
		}
		
		#endregion


        #region ICamera Members

        public event EventHandler<CameraStateChangedEventArgs>? StateChanged;

        public event EventHandler<CameraBatteryWarningEventArgs>? BatteryWarning;

        #endregion
    }
}
