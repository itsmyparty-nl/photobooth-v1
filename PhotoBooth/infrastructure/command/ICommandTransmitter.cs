/*
 *Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.infrastructure.command
{
    public interface ICommandTransmitter: IHardwareController, IDisposable
    {
        void SendCommand(CommandType buttonType, string context, string value);
    }
}