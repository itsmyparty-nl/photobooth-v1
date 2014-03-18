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

using System;
using System.Globalization;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class RemoteControl : IRemoteControl
    {
        private readonly ICommandReceiver commandReceiver;
        private readonly ICommandTransmitter commandTransmitter;
        private readonly ILogger logger;
        private readonly Command command;
        private bool prepared;

        public RemoteControl(Command command,
            ICommandReceiver commandReceiver, ICommandTransmitter commandTransmitter, ILogger logger)
        {
            Id = command.ToString();
            this.command = command;
            this.commandReceiver = commandReceiver;
            this.commandTransmitter = commandTransmitter;
            this.logger = logger;

            commandReceiver.Subscribe(command);
            prepared = false;
        }

        #region IRemoteControl Members

        public string Id { get; private set; }

        public void Prepare()
        {
            if (prepared)
            {
                logger.LogInfo(String.Format(CultureInfo.InvariantCulture,
                                             "RemoteControl.{0}: Not allowed to prepare twice", Id));
            }
            commandReceiver.CommandReceived += OnCommandReceived;
            commandTransmitter.SendCommand(Command.PrepareControl, Id, string.Empty);
            
            prepared = true;
        }

        public void Release()
        {
            if (!prepared)
            {
                logger.LogInfo(String.Format(CultureInfo.InvariantCulture,
                                             "RemoteControl.{0}: Cannot release when not prepared", Id));
            }
            commandReceiver.CommandReceived -= OnCommandReceived;
            commandTransmitter.SendCommand(Command.ReleaseControl, Id, string.Empty);

            prepared = false;
        }

        public event EventHandler<RemoteControlEventArgs> Triggered;

        #endregion

        private void OnCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            //Only handle a single button type: the one that's configured
            if (e.Command.Equals(command) && Triggered != null)
            {
               Triggered.Invoke(this, new RemoteControlEventArgs(Id)); 
            }
        }

        	#region IDisposable Implementation

		bool disposed;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// Clean up managed objects
					
				}
				// clean up any unmanaged objects
				disposed = true;
			} else {
				Console.WriteLine ("Saved us from doubly disposing an object!");
			}
		}

		~RemoteControl()
		{
			Dispose (false);
		}
		
		#endregion
    }
}
