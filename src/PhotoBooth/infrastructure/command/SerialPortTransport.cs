using com.prodg.photobooth.config;
using CommandMessenger;
using CommandMessenger.TransportLayer;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.command;

public class SerialPortTransport: SerialTransport
{
    public SerialPortTransport(ISettings settings, ILogger<SerialPortTransport> logger)
    {
        CurrentSerialSettings = new SerialSettings
        {
            PortName = settings.SerialPortName,
            BaudRate = settings.SerialPortBaudRate,
            DtrEnable = settings.SerialPortDtrEnable
        };
        logger.LogInformation("Creating serial transport {SerialPortName}:{SerialPortBaudRate}:{SerialPortDtrEnable}",
            CurrentSerialSettings.PortName, CurrentSerialSettings.BaudRate, CurrentSerialSettings.DtrEnable);
    }
}