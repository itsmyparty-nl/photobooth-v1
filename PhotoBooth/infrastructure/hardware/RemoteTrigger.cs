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
using System.Collections.Generic;
using System.Globalization;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
    /// <summary>
    /// The states in which the trigger can reside
    /// </summary>
    internal enum TriggerState
    {
        Released,
        Armed,
    }

    /// <summary>
    /// The remote trigger
    /// <para>
    /// This class represents a trigger (e.g. a button) which is present remotely. It passes commands over via
    /// a transmitter and receives commands via a receiver.
    /// </para>
    /// </summary>
    public class RemoteTrigger : ITriggerControl
    {
        private readonly ICommandReceiver commandReceiver;
        private readonly ICommandTransmitter commandTransmitter;
        private readonly ILogger logger;
        private readonly Command command;
        private readonly string triggerContext;
        private TriggerState currentState;
        private readonly Random lockIdGenerator;

        private readonly List<int> lockQueue;

        private bool Locked
        {
            get { return lockQueue.Count > 0; }
        }

    /// <summary>
        /// C'tor
        /// </summary>
        public RemoteTrigger(Command command,
            ICommandReceiver commandReceiver, ICommandTransmitter commandTransmitter, ILogger logger)
        {
            Id = command.ToString();
            triggerContext = ((int) command).ToString(CultureInfo.InvariantCulture);
            lockQueue = new List<int>();
            lockIdGenerator = new Random();
            this.command = command;
            this.commandReceiver = commandReceiver;
            this.commandTransmitter = commandTransmitter;
            this.logger = logger;

        }

        #region IRemoteControl Members

        public string Id { get; private set; }

        public void Arm()
        {
            if (!ChangeState(TriggerState.Armed)) return;
            
            commandReceiver.CommandReceived += OnCommandReceived;
            if (!Locked)
            {
                commandTransmitter.SendCommand(Command.PrepareControl, triggerContext);
            }
        }

        public void Release()
        {
            if (!ChangeState(TriggerState.Released)) return;
            
            commandReceiver.CommandReceived -= OnCommandReceived;
            if (!Locked)
            {
                commandTransmitter.SendCommand(Command.ReleaseControl, triggerContext);
            }
        }

        public int Lock(bool indicateLock)
        {
            //first send a release to the control to make sure any active component on the remote side is
            //gracefully stopped. Note that this does not actually release the control!
            commandTransmitter.SendCommand(Command.ReleaseControl, triggerContext);

            if (indicateLock)
            {
                commandTransmitter.SendCommand(Command.LockControl, triggerContext);
            }

            var lockId = lockIdGenerator.Next();
            lockQueue.Add(lockId);
            return lockId;
        }

        public void Unlock(int lockId)
        {
            if (!lockQueue.Contains(lockId))
            {
                logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                    "RemoteControl.{0}: trying to unlock with non-locked id ({1})", Id, lockId));
                return;
            }
            
            commandTransmitter.SendCommand(Command.UnlockControl, triggerContext);
            switch (currentState)
            {
                case TriggerState.Released:
                    commandTransmitter.SendCommand(Command.ReleaseControl, triggerContext);
                    break;
                case TriggerState.Armed:
                    commandTransmitter.SendCommand(Command.PrepareControl, triggerContext);
                    break;
                default:
                    throw new NotSupportedException("TriggerState not supported: " + currentState);
            }
            lockQueue.Remove(lockId);
        }

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        private void OnCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            //Only handle a single button type: the one that's configured
            if (Locked)
            {
                logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                            "RemoteControl.{0}: Ignoring received command due to lock", Id));
                return;
            }

            if (e.Command.Equals(command) && Fired != null)
            {
               Fired.Invoke(this, new TriggerControlEventArgs(Id)); 
            }
        }

        private bool ChangeState(TriggerState newState)
        {
            if (newState.Equals(currentState))
            {
                 logger.LogDebug(String.Format(CultureInfo.InvariantCulture,
                                             "RemoteControl.{0}: Ignoring transfer to identical trigger state", Id));
                return false;
            }

            switch (currentState)
            {
                case TriggerState.Released:
                    switch (newState)
                    {
                        case TriggerState.Armed:
                            break;
                        default:
                            throw new NotSupportedException("TriggerState not supported: " + newState);
                    }
                    break;
                case TriggerState.Armed:
                    switch (newState)
                    {
                        case TriggerState.Released:
                            break;
                        default:
                            throw new NotSupportedException("TriggerState not supported: " + newState);
                    }
                    break;
                default:
                    throw new NotSupportedException("TriggerState not supported: " + currentState);
            }

            //If the state transition is allowed, perform the transition
            currentState = newState;
            return true;
        }


        #region IHardwareController Members

        public void Initialize()
        {
            currentState = TriggerState.Released;
            lockQueue.Clear();

            //Subscribe to the specific command handled by this control
            commandReceiver.Subscribe(command);
            commandTransmitter.SendCommand(Command.ReleaseControl, triggerControl);
        }

        public void DeInitialize()
        {
            //First release
            Release();

            commandReceiver.UnSubscribe(command);
        }

        #endregion
    }
}
