using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.infrastructure.command;

public interface IConsoleCommandReceiver: ICommandReceiver, IHardwareController, IDisposable
{
}