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

using System.Globalization;
using com.prodg.photobooth.infrastructure.command;
using Microsoft.Extensions.Logging;

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
        private readonly ICommandReceiver _commandReceiver;
        private readonly ICommandTransmitter _commandTransmitter;
        private readonly ILogger<RemoteTrigger> _logger;
        private readonly Command _command;
        private readonly string _triggerContext;
        private TriggerState _currentState;
        private readonly Random _lockIdGenerator;

        private readonly List<int> _lockQueue;

        private bool Locked => _lockQueue.Count > 0;

        public RemoteTrigger(Command command,
            ICommandReceiver commandReceiver, ICommandTransmitter commandTransmitter, ILogger<RemoteTrigger> logger)
        {
            Id = command.ToString();
            _triggerContext = ((int) command).ToString(CultureInfo.InvariantCulture);
            _lockQueue = new List<int>();
            _lockIdGenerator = new Random();
            _command = command;
            _commandReceiver = commandReceiver;
            _commandTransmitter = commandTransmitter;
            _logger = logger;
        }

        #region IRemoteControl Members

        public string Id { get; }

        public void Arm()
        {
            if (!ChangeState(TriggerState.Armed)) return;
            
            _commandReceiver.CommandReceived += OnCommandReceived!;
            if (!Locked)
            {
                _commandTransmitter.SendCommand(Command.PrepareControl, _triggerContext);
            }
        }

        public void Release()
        {
            if (!ChangeState(TriggerState.Released)) return;
            
            _commandReceiver.CommandReceived -= OnCommandReceived!;
            if (!Locked)
            {
                _commandTransmitter.SendCommand(Command.ReleaseControl, _triggerContext);
            }
        }

        public int Lock(bool indicateLock)
        {
            //first send a release to the control to make sure any active component on the remote side is
            //gracefully stopped. Note that this does not actually release the control!
            _commandTransmitter.SendCommand(Command.ReleaseControl, _triggerContext);

            if (indicateLock)
            {
                _commandTransmitter.SendCommand(Command.LockControl, _triggerContext);
            }

            var lockId = _lockIdGenerator.Next();
            _lockQueue.Add(lockId);
            return lockId;
        }

        public void Unlock(int lockId)
        {
            if (!_lockQueue.Contains(lockId))
            {
                _logger.LogDebug("RemoteControl {Id}: trying to unlock with non-locked id ({LockId})", Id, lockId);
                return;
            }
            
            _commandTransmitter.SendCommand(Command.UnlockControl, _triggerContext);
            switch (_currentState)
            {
                case TriggerState.Released:
                    _commandTransmitter.SendCommand(Command.ReleaseControl, _triggerContext);
                    break;
                case TriggerState.Armed:
                    _commandTransmitter.SendCommand(Command.PrepareControl, _triggerContext);
                    break;
                default:
                    throw new NotSupportedException("TriggerState not supported: " + _currentState);
            }
            _lockQueue.Remove(lockId);
        }

        public event EventHandler<TriggerControlEventArgs> Fired;

        #endregion

        private void OnCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            //Only handle a single button type: the one that's configured
            if (Locked)
            {
                _logger.LogDebug("RemoteControl {Id}: Ignoring received command due to lock", Id);
                return;
            }

            if (e.Command.Equals(_command))
            {
               Fired.Invoke(this, new TriggerControlEventArgs(Id)); 
            }
        }

        private bool ChangeState(TriggerState newState)
        {
            if (newState.Equals(_currentState))
            {
                 _logger.LogDebug("RemoteControl {Id}: Ignoring transfer to identical trigger state", Id);
                return false;
            }

            switch (_currentState)
            {
                case TriggerState.Released:
                    switch (newState)
                    {
                        case TriggerState.Armed:
                            break;
                        case TriggerState.Released:
                        default:
                            throw new NotSupportedException("TriggerState not supported: " + newState);
                    }
                    break;
                case TriggerState.Armed:
                    switch (newState)
                    {
                        case TriggerState.Released:
                            break;
                        case TriggerState.Armed:
                        default:
                            throw new NotSupportedException("TriggerState not supported: " + newState);
                    }
                    break;
                default:
                    throw new NotSupportedException("TriggerState not supported: " + _currentState);
            }

            //If the state transition is allowed, perform the transition
            _currentState = newState;
            return true;
        }


        #region IHardwareController Members

        public void Initialize()
        {
            _currentState = TriggerState.Released;
            _lockQueue.Clear();

            //Subscribe to the specific command handled by this control
            _commandReceiver.Subscribe(_command);
            _commandTransmitter.SendCommand(Command.ReleaseControl, _triggerContext);
        }

        public void DeInitialize()
        {
            //First release
            Release();

            _commandReceiver.UnSubscribe(_command);
        }

        #endregion
    }
}
