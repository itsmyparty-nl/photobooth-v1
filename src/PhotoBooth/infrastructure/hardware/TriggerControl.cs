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

using System.Diagnostics;
using System.Globalization;
using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
    /// <summary>
    /// The remote trigger
    /// <para>
    /// This class represents a trigger (e.g. a button) which is present remotely. It passes commands over via
    /// a transmitter and receives commands via a receiver.
    /// </para>
    /// </summary>
    public class TriggerControl : ITriggerControl
    {
        private readonly Random _lockIdGenerator;
        private readonly List<int> _lockQueue;
        private readonly Command _command;
        private readonly string _id;
        private readonly string _context;

        public bool Locked => _lockQueue.Count > 0;

        public event EventHandler<TriggerStateEventArgs>? StateChanged;

        public event EventHandler<TriggerCommandEventArgs>? Triggered;
        
        public event EventHandler<TriggerControlEventArgs>? Fired;
        
        public TriggerControl(Command command)
        {
            _command = command;
            _id = command.ToString();
            _context = ((int) command).ToString(CultureInfo.InvariantCulture);
            
            _lockQueue = new List<int>();
            _lockIdGenerator = new Random();

        }
        
        #region IRemoteControl Members

        public Command Command => _command;

        public string Id => _id;

        public TriggerState State { get; private set; }

        public void Arm()
        {
            if (!ChangeState(TriggerState.Armed)) return;
            
            SignalStateChange();
        }

        public void Release()
        {
            if (!ChangeState(TriggerState.Released)) return;

            SignalStateChange();
        }

        public void Fire()
        {
            if (Locked)
            {
                Debug.Assert(!Locked,$"Ignoring fire for locked control '{Id}'");
                return;
            }
            Debug.Assert(Fired!=null, $"No registered listeners for control '{Id}' fire event");
            Fired?.Invoke(this, new TriggerControlEventArgs(Id));
        }
        
        public int Lock(bool indicateLock)
        {
            //first send a release to the control to make sure any active component on the remote side is
            //gracefully stopped. Note that this does not actually release the control!
            Trigger(Command.ReleaseControl);

            if (indicateLock)
            {
                Trigger(Command.LockControl);
            }

            var lockId = _lockIdGenerator.Next();
            _lockQueue.Add(lockId);
            return lockId;
        }

        public void Unlock(int lockId)
        {
            if (!_lockQueue.Contains(lockId))
            {
                Debug.Assert(true,$"RemoteControl {Id}: trying to unlock with non-locked id ({lockId})");
                return;
            }
            
            Trigger(Command.UnlockControl);
            //Upon unlocking, ensure the state is applied
            switch (State)
            {
                case TriggerState.Released:
                    Trigger(Command.ReleaseControl);
                    break;
                case TriggerState.Armed:
                    Trigger(Command.PrepareControl);
                    break;
                default:
                    throw new NotSupportedException("TriggerState not supported: " + State);
            }
            _lockQueue.Remove(lockId);
        }
        
        #endregion
        
        #region IHardwareController Members

        public void Initialize()
        {
            _lockQueue.Clear();
            Release();
        }

        public void DeInitialize()
        {
            _lockQueue.Clear();
            Release();
        }

        #endregion
        
        private bool ChangeState(TriggerState newState)
        {
            if (newState.Equals(State))
            {
                return false;
            }

            switch (State)
            {
                case TriggerState.Released:
                    switch (newState)
                    {
                        case TriggerState.Armed:
                            break;
                        case TriggerState.Released:
                        default:
                            throw new NotSupportedException($"State transition from '{State}' to '{newState}' not supported");
                    }
                    break;
                case TriggerState.Armed:
                    switch (newState)
                    {
                        case TriggerState.Released:
                            break;
                        case TriggerState.Armed:
                        default:
                            throw new NotSupportedException($"State transition from '{State}' to '{newState}' not supported");
                    }
                    break;
                default:
                    throw new NotSupportedException($"TriggerState not supported: {State}");
            }

            //If the state transition is allowed, perform the transition
            State = newState;
            return true;
        }
        
        private void Trigger(Command command)
        {
            Debug.Assert(Triggered!=null, "No trigger listener configured for remote trigger '{Id}'" );
            Triggered?.Invoke(this, new TriggerCommandEventArgs(_context, command));
        }
        
        private void SignalStateChange()
        {
            if (!Locked)
            {
                Debug.Assert(StateChanged!=null, "No trigger listener configured for remote trigger '{Id}'" );
                StateChanged?.Invoke(this, new TriggerStateEventArgs(_context, State));
            }
        }
    }
}
