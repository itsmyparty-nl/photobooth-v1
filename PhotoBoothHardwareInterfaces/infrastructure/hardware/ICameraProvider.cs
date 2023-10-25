namespace com.prodg.photobooth.infrastructure.hardware;

public class CameraInfo
{
    public string Id {get;}

    public string Model {get;}

    public string Status {get;}

    public CameraInfo(string id, string model, string status)
    {
        Id = id;
        Model = model;
        Status = status;
    }
}

public interface ICameraProvider
{
    string Id { get; }
    CameraInfo? Info { get; }
    bool Initialized { get; }
    void Initialize ();

    /// <remarks>In all cases that no value can be retrieved a default battery level is returned which
    /// indicates a full battery. rationale is that in this case manual checking is needed, and the
    /// software should work without issues</remarks>
    int GetBatteryLevel();

    bool Capture(string capturePath);
    void Clean ();
}