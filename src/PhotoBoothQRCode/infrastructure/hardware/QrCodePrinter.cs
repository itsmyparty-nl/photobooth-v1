using com.prodg.photobooth.domain.image;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace com.prodg.photobooth.infrastructure.hardware;

public class QrCodePrinter: IPrinter
{
    private readonly ILogger<QrCodePrinter> _logger;
    private readonly QrCodeOverlayImageProcessor _processor;
    private Image? _lastPrint;
    private readonly object _locker;

    public QrCodePrinter(ILogger<QrCodePrinter> logger, QrCodeOverlayImageProcessor processor)
    {
        _logger = logger;
        _processor = processor;
        _locker = new object();
    }
    
    public void Initialize()
    {
        _logger.LogInformation("Initialize");
    }

    public void DeInitialize()
    {
        _logger.LogInformation("DeInitialize");
    }

    public void Print(Image image, string eventId, int sessionIndex)
    {
        _logger.LogInformation("Print - {0}, {1}", eventId, sessionIndex);

        try
        {
            lock (_locker)
            {
                _lastPrint = _processor.Process(image, sessionIndex);
            }
        }
        catch (Exception e)
        {
           _logger.LogError(e, "Error while printing");
           _lastPrint = image;
        }
    }

    public Image? GetLastPrint()
    {
        lock (_locker)
        {
            return _lastPrint;
        }
    }
}