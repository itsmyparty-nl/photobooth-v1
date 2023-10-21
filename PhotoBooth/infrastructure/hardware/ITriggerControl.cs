#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
/*
  PhotoBooth - an application to control a DIY photobooth

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
  
  Copyright 2014 Patrick Bronneberg
*/
#endregion

using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class TriggerControlEventArgs : EventArgs
    {
        public TriggerControlEventArgs(string id)
		{
			RemoteControlId = id;
		}
		
		public string RemoteControlId {get; }
    }
	
	/// <summary>
	/// The states in which the trigger can reside
	/// </summary>
	public enum TriggerState
	{
		Released,
		Armed,
	}

	public class TriggerStateEventArgs : TriggerControlEventArgs
	{
		public TriggerStateEventArgs(string id, TriggerState triggerState) :
			base(id)
		{
			TriggerState = triggerState;
		}

		public TriggerState TriggerState { get; private set; }
	}
    
	public class TriggerCommandEventArgs : TriggerControlEventArgs
	{
		public TriggerCommandEventArgs(string id, Command command):
			base(id)
		{
			Command = command;
		}

		public Command Command { get; private set; }
	}
	
	public interface ITriggerControl: IHardwareController
	{
		Command Command { get; }
		
		string Id {get;}
		
		bool Locked { get; }
		
		void Arm();
		
		void Release();

	    int Lock(bool indicateLock);

	    void Unlock(int lockId);

	    void Fire();
		
		event EventHandler<TriggerControlEventArgs>? Fired;
		
		event EventHandler<TriggerCommandEventArgs>? Triggered;

		event EventHandler<TriggerStateEventArgs>? StateChanged;
		
	}
}
