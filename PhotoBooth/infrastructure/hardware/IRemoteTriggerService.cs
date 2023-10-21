namespace com.prodg.photobooth.infrastructure.hardware;

public interface IRemoteTriggerService
{
    void Register(ITriggerControl triggerControl);
    
    void DeRegister(ITriggerControl triggerControl);
}