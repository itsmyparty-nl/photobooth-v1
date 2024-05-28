using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.infrastructure.command;

public interface ICommandMessengerTransceiver:  ICommandReceiver, ICommandTransmitter, IHardwareController, IDisposable
{
    //Interface introduced to pin implementation
}