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
    public class RemoteTrigger : ITriggerControl
    {
        private readonly ICommandReceiver commandReceiver;
        private readonly ICommandTransmitter commandTransmitter;
        private readonly ILogger logger;
        private readonly Command command;
        private bool prepared;

        public RemoteTrigger(Command command,
            ICommandReceiver commandReceiver, ICommandTransmitter commandTransmitter, ILogger logger)
        {
            Id = command.ToString();
            this.command = command;
            this.commandReceiver = commandReceiver;
            this.commandTransmitter = commandTransmitter;
            this.logger = logger;

            prepared = false;
        }

        #region IRemoteControl Members

        public string Id { get; private set; }

        public void ArmTrigger()
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

        public void ReleaseTrigger()
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

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        private void OnCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            //Only handle a single button type: the one that's configured
            if (e.Command.Equals(command) && Fired != null)
            {
               Fired.Invoke(this, new TriggerControlEventArgs(Id)); 
            }
        }

        #region IHardwareController Members

        public void Initialize()
        {
            //Subscribe to the specific command handled by this control
            commandReceiver.Subscribe(command);
        }

        public void DeInitialize()
        {
            //Do nothing yet
            commandReceiver.UnSubscribe(command);
        }

        #endregion
    }
}
