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
using SixLabors.ImageSharp.Drawing.Processing;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class StubCameraProvider : ICameraProvider
	{
		private readonly ILogger<StubCameraProvider> _logger;

	    public string Id { get; }

	    public CameraInfo? Info {get; private set;}
	    
	    public bool Initialized => _initialized;

	    private bool _initialized;

	    private readonly Image<Rgb24> _dummyImage;

		public StubCameraProvider (ILogger<StubCameraProvider> logger)
		{
			Id = "Uninitialized";
			_logger = logger;
			_dummyImage = new Image<Rgb24>(1500, 1000);
			_dummyImage.Mutate(x => x.Fill(Color.Plum));
		}

		public void Initialize ()
		{
			_logger.LogInformation("Initializing DUMMY camera");
			Info = new CameraInfo("Dummy Id", "Dummy Model", "Dummy Status");
			
			_initialized = true;
		}

		public int GetBatteryLevel()
		{
			return new Random().Next(25, 99);
		}

		public void DeInitialize()
	    {
            _logger.LogInformation("DeInitializing DUMMY camera");
            _initialized = false;
	    }

	    public bool Capture(string capturePath)
	    {
	        _logger.LogInformation("Starting capture to: {Path}", capturePath);
	        _dummyImage.Save(capturePath);
	        return true;
	    }

	    public void Clean ()
		{
	        _logger.LogInformation("Cleaning camera");
		}
	}
}
