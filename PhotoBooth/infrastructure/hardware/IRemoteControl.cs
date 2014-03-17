/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class RemoteControlEventArgs : EventArgs
    {
        public RemoteControlEventArgs(string id)
		{
			RemoteControlId = id;
		}
		
		public string RemoteControlId {get; private set;}
    }
	
	public interface IRemoteControl: IDisposable
	{
		string Id {get;}
		
		void Prepare();
		
		void Release();
		
		event EventHandler<RemoteControlEventArgs> Triggered;
	}
}
