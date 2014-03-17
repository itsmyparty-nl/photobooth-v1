/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
namespace com.prodg.photobooth.infrastructure.hardware
{
    public interface IHardware: IDisposable
    {
        ICamera Camera { get;}

        IRemoteControl TriggerControl { get;}
        
        IRemoteControl PrintControl { get;}

        IRemoteControl PowerControl { get;}

        void Initialize();

        void Release();
    }
}
