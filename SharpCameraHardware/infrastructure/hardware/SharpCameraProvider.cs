﻿using Microsoft.Extensions.Logging;
using SharpCamera;

namespace com.prodg.photobooth.infrastructure.hardware;

public class SharpCameraProvider: ICameraProvider
{
    private TetheredCamera? _camera;
    private readonly ILogger<SharpCameraProvider> _logger;
    public string Id { get; private set; }
    public CameraInfo? Info { get; private set; }
    public bool Initialized { get; private set; }

    public SharpCameraProvider(ILogger<SharpCameraProvider> logger)
    {
        Id = "Uninitialized";
        _logger = logger;
        Initialized = false;
    }
    public void Initialize()
    {
        try
        {
            List<TetheredCamera> cams = TetheredCamera.Scan();
            foreach (TetheredCamera c in cams)
            {
                _logger.LogInformation("Found camera: {Camera}", c);
            }
            

            if (!cams.Any())
            {
                _logger.LogWarning("No camera found");
            }
            _camera = cams[0];

            Info = new CameraInfo(_camera.Name, _camera.Name, _camera.USBPort);

            Id = Info.Id;
            
            _camera.Connect();
            Initialized = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error while initializing camera");
            Initialized = false;
        }
        
    }

    public int GetBatteryLevel()
    {
        return _camera is {Connected: true} ? 100 : 0;
    }

    public bool Capture(string capturePath)
    {
        if (_camera == null || !_camera.Connected)
        {
            _logger.LogError("Camera not connected");
            return false;
        }

        try
        {
            File.WriteAllBytes(capturePath, _camera.CaptureAsBytes());
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while capturing '{CapturePath}'",capturePath);
            return false;
        }
    }

    public void Clean()
    {
        if (_camera == null || !_camera.Connected)
        {
            _logger.LogError("Camera not initialized");
            return;
        }
        _camera.Exit();
        Initialize();
    }
}