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
using com.prodg.photobooth.infrastructure.command;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.hardware
{
    /// <summary>
    /// The remote trigger
    /// <para>
    /// This class represents a trigger (e.g. a button) which is present remotely. It passes commands over via
    /// a transmitter and receives commands via a receiver.
    /// </para>
    /// </summary>
    public class RemoteTriggerService : IRemoteTriggerService, IDisposable
    {
        private readonly ICommandReceiver _commandReceiver;
        private readonly ICommandTransmitter _commandTransmitter;
        private readonly ILogger<RemoteTriggerService> _logger;

        private readonly IList<ITriggerControl> _triggerControls;
        
        public RemoteTriggerService(
            ICommandMessengerTransceiver transceiver, ILogger<RemoteTriggerService> logger)
        {
            _commandReceiver = transceiver;
            _commandTransmitter = transceiver;
            _logger = logger;
            _triggerControls = new List<ITriggerControl>();
            
            _commandReceiver.CommandReceived += OnCommandReceived;
        }

        public void Register(ITriggerControl triggerControl)
        {
            if (_triggerControls.Contains(triggerControl)){ return; }
            _logger.LogDebug($"Registering control '{triggerControl.Id}'");
            
            _triggerControls.Add(triggerControl);
            _commandReceiver.Subscribe(triggerControl.Command);
            triggerControl.StateChanged += TriggerControlOnStateChanged;
            triggerControl.Triggered += TriggerControlOnTriggered;
        }

        public void DeRegister(ITriggerControl triggerControl)
        {
            if (!_triggerControls.Contains(triggerControl))
            {
                Debug.Assert(false, $"Trying to deregister non-registered control '{triggerControl.Id}'");
                return;
            }

            _logger.LogDebug($"DeRegistering control '{triggerControl.Id}'");

            triggerControl.StateChanged -= TriggerControlOnStateChanged;
            triggerControl.Triggered -= TriggerControlOnTriggered;
            _commandReceiver.UnSubscribe(triggerControl.Command);
            _triggerControls.Remove(triggerControl);
        }

        private void TriggerControlOnTriggered(object? sender, TriggerCommandEventArgs e)
        {
            Debug.Assert(e!=null, "Control triggered without event");
            _logger.LogDebug($"Control '{e.RemoteControlId}' triggered command '{e.Command}'");

            _commandTransmitter.SendCommand(e.Command, e.RemoteControlId);
        }

        private void TriggerControlOnStateChanged(object? sender, TriggerStateEventArgs e)
        {
            Debug.Assert(e!=null, "State change triggered without event");
            _logger.LogDebug($"Control '{e.RemoteControlId}' changed state to '{e.TriggerState}'");

            var command = MapCommandToTriggerState(e.TriggerState);
            _commandTransmitter.SendCommand(command, e.RemoteControlId);
        }

        private static Command MapCommandToTriggerState(TriggerState triggerState)
        {
            switch (triggerState)
            {
                case TriggerState.Armed:
                    return Command.PrepareControl;
                case TriggerState.Released:
                    return Command.ReleaseControl;
                default:
                    throw new NotSupportedException($"No supported command for state '{triggerState}'");
            }
        }

        private void OnCommandReceived(object? sender, CommandReceivedEventArgs e)
        {
            Debug.Assert(sender!=null, "Command received from unknown sender");
            Debug.Assert(e!=null, "Received command without event arguments");
            _logger.LogTrace($"Received command '{e.Command}'");
            
            foreach (var control in _triggerControls.Where(control => control.Command.Equals(e.Command)))
            {
                //Only handle a single button type: the one that's configured
                if (control.Locked)
                {
                    _logger.LogDebug("RemoteControl {Id}: Ignoring received command due to lock", control.Id);
                    continue;
                }
                control.Fire();
            }
        }

        private void ReleaseUnmanagedResources()
        {
            _triggerControls.Clear();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~RemoteTriggerService()
        {
            ReleaseUnmanagedResources();
        }
    }
}
