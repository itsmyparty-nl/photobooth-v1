using Microsoft.Extensions.Logging;
using SharpCamera;

namespace com.prodg.photobooth.infrastructure.hardware;

public class SharpCameraProvider: ICameraProvider
{
    private int _reconnectionAttempts = 0;
    
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
        _logger.LogInformation("Initialize Camera: attempt {0}", _reconnectionAttempts+1);
        try
        {
            if (_camera != null && _camera.Connected)
            {
                _logger.LogInformation("Try exiting connected camera before init");
                try
                {
                    _camera.Exit();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Camera exit failed");
                }
            }
            
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
            _reconnectionAttempts = 0;
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error while initializing camera");
            Initialized = false;
            _reconnectionAttempts++;
            if (_reconnectionAttempts < 3)
            {
                Thread.Sleep(500*_reconnectionAttempts);
                Initialize();
            }
        }
        
    }

    public int GetBatteryLevel()
    {
        return _camera is {Connected: true} ? 100 : 0;
    }

    public bool Capture(string capturePath)
    {
        _logger.LogInformation("Capture {0}", capturePath);
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
        _logger.LogInformation("Cleaning camera");
        if (_camera == null || !_camera.Connected)
        {
            _logger.LogWarning("Camera not initialized");
        }
        else
        {

            try
            {
                _camera.Exit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot Exit camera on Clean");
            }
        }
        Initialize();
    }
}