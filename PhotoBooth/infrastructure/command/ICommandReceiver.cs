/*
 *Copyright 2014 Patrick Bronneberg
 * 
*/

using System;
using com.prodg.photobooth.infrastructure.hardware;

namespace com.prodg.photobooth.infrastructure.command
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public CommandReceivedEventArgs(CommandType command)
        {
            Command = command;
        }

        public CommandType Command { get; private set; }
    }
   
    public interface ICommandReceiver : IHardwareController, IDisposable
    {
        event EventHandler<CommandReceivedEventArgs> CommandReceived;
        
        void Subscribe(CommandType command);
    }
}
