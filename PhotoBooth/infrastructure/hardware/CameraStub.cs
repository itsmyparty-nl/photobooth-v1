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
	public class CameraStub : ICamera
	{
		private readonly ILogger<CameraStub> _logger;

        public event EventHandler<CameraStateChangedEventArgs> StateChanged;
	    public event EventHandler<CameraBatteryWarningEventArgs> BatteryWarning;
	    public string Id { get; }

		public CameraStub (ILogger<CameraStub> logger)
		{
			_logger = logger;
		    Id = "Dummy";
		}

		public void Initialize ()
		{
			_logger.LogInformation("Initializing DUMMY camera");
		}

	    public void DeInitialize()
	    {
            _logger.LogInformation("DeInitializing DUMMY camera");
	    }

	    public bool Capture(string capturePath)
	    {
	        _logger.LogInformation("Starting capture to: {Path}", capturePath);
	        return true;
	    }

	    public void Clean ()
		{
	        _logger.LogInformation("Cleaning camera");
		}
	}
}
