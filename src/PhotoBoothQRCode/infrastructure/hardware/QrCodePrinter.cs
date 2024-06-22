using com.prodg.photobooth.domain.image;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace com.prodg.photobooth.infrastructure.hardware;

public class QrCodePrinter: IPrinter
{
    private readonly ILogger<QrCodePrinter> _logger;
    private QrCodeOverlayImageProcessor _processor;
    private readonly string _baseUrl;
    private Image? _lastPrint;

    public QrCodePrinter(ILogger<QrCodePrinter> logger, QrCodeOverlayImageProcessor processor)
    {
        _logger = logger;
        _processor = processor;
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
            _lastPrint = _processor.Process(image, sessionIndex);
        }
        catch (Exception e)
        {
           _logger.LogError(e, "Error while printing");
           _lastPrint = image;
        }
    }

    public Image? GetLastPrint()
    {
        return _lastPrint;
    }
}