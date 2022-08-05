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

  Based on CmdMessenger sample code Copyright 2013 Thijs Elenbaas
  
  Copyright 2014 Patrick Bronneberg
*/
#endregion

using CommandMessenger;
using CommandMessenger.TransportLayer;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.command
{
    /// <summary>
    /// Command Messenger Transceiver
    /// <para>
    /// This class acts as an adapter between the photobooth application and the command messenger
    /// </para>
    /// </summary>
    public class CommandMessengerTransceiver : ICommandReceiver, ICommandTransmitter
    {
        private readonly ILogger<CommandMessengerTransceiver> _logger;
        private bool _running;

        private readonly List<Command> _subscriptions = new();
        private readonly CmdMessenger _messenger;

        public CommandMessengerTransceiver(ILogger<CommandMessengerTransceiver> logger, ITransport transport)
        {
            _logger = logger;
            
            // Initialize the command messenger with the Serial Port transport layer
            _messenger = new CmdMessenger(transport) {PrintLfCr = true};

            // Attach to NewLinesReceived for logging purposes
            _messenger.NewLineReceived += NewLineReceived!;
            // Attach to NewLineSent for logging purposes
            _messenger.NewLineSent += NewLineSent!;

            AttachCommandCallBacks();

            _messenger.StartListening();
        }

        #region ICommandReceiver Members

        public event EventHandler<CommandReceivedEventArgs>? CommandReceived;

        public void Subscribe(Command command)
        {
            _subscriptions.Add(command);
        }

        public void UnSubscribe(Command command)
        {
            _subscriptions.Remove(command);
        }

        #endregion

        #region IHardwareController Members

        public void Initialize()
        {
            if (_running)
            {
                throw new InvalidOperationException("Not allowed to start twice");
            }

            _logger.LogInformation("Starting serial command transceiver");
            // Start listening
            _messenger.StartListening();

            SendCommand(Command.Initialize, "Initialize");
            _running = true;
        }

        public void DeInitialize()
        {
            _logger.LogInformation("Stopping serial command transceiver");

            SendCommand(Command.Initialize, "DeInitialize");

            _messenger.StopListening();
            _running = false;
        }

        #endregion

        #region IDisposable Implementation

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clean up managed objects
                    DeInitialize();

                    _messenger.Dispose();
                }
                // clean up any unmanaged objects
                _disposed = true;
            }
            else
            {
                Console.WriteLine("Saved us from doubly disposing an object!");
            }
        }

        ~CommandMessengerTransceiver()
        {
            Dispose(false);
        }

        #endregion

        /// Attach command call backs. 
        private void AttachCommandCallBacks()
        {
            _messenger.Attach(OnReceivedCommand);
            _messenger.Attach((int)Command.Acknowledge, OnAcknowledge);
            _messenger.Attach((int)Command.Error, OnError);
        }

        // Called when a received command has no internally attached function.
        void OnReceivedCommand(ReceivedCommand arguments)
        {
            try
            {
                var command = (Command) arguments.CmdId;
                if (_subscriptions.Contains(command))
                {
                    _logger.LogDebug("Received Command: {Command}", command);

                    CommandReceived?.Invoke(this, new CommandReceivedEventArgs(command));

                }
                else
                {
                    _logger.LogWarning("Received command without subscribers: {Command}", command);
                }
            }
            catch (InvalidCastException)
            {
                _logger.LogWarning("Received unsupported command: {Arguments}", arguments.rawString);
            }
        }

        #region ICommandTransmitter Members

        public void SendCommand(Command commandType, string context)
        {
            // Create command FloatAddition, which will wait for a return command FloatAdditionResult
            var command = new SendCommand((int)commandType, context);

            // Send command
            _messenger.SendCommand(command);

            _logger.LogDebug("Sent Command: {CommandType}", commandType);
        }

        #endregion

        // Callback function that prints that the Arduino has acknowledged
        private void OnAcknowledge(ReceivedCommand arguments)
        {
            _logger.LogDebug("Acknowledged: {Command}", arguments.CmdId);
        }

        // Callback function that prints that the Arduino has experienced an error
        private void OnError(ReceivedCommand arguments)
        {
            _logger.LogError("Remote side has experienced an error");
        }

        // Log received line to console
        private void NewLineReceived(object sender, EventArgs e)
        {
            _logger.LogDebug("Received > {Received}", _messenger.CurrentReceivedLine);
        }

        // Log sent line to console
        private void NewLineSent(object sender, EventArgs e)
        {
            _logger.LogDebug("Sent > {Sent}", _messenger.CurrentSentLine);
        }
    }
}
