﻿#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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

using System;
using System.Collections.Generic;
using System.Globalization;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using CommandMessenger;
using CommandMessenger.TransportLayer;

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
        private readonly ILogger logger;
        private bool running;

        private readonly List<Command> subscriptions = new List<Command>();
        private CmdMessenger messenger;
        private ITransport transport;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="transport"></param>
        public CommandMessengerTransceiver(ILogger logger, ITransport transport)
        {
            this.logger = logger;
            
            // Initialize the command messenger with the Serial Port transport layer
            messenger = new CmdMessenger(transport);
           
            // Attach to NewLinesReceived for logging purposes
            messenger.NewLineReceived += NewLineReceived;
            // Attach to NewLineSent for logging purposes
            messenger.NewLineSent += NewLineSent;

            AttachCommandCallBacks();

            messenger.StartListening();
        }

        #region ICommandReceiver Members

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        public void Subscribe(Command command)
        {
            subscriptions.Add(command);
        }

        public void UnSubscribe(Command command)
        {
            subscriptions.Remove(command);
        }

        #endregion

        #region IHardwareController Members

        public void Initialize()
        {
            if (running)
            {
                throw new InvalidOperationException("Not allowed to start twice");
            }

            logger.LogInfo("Starting serial command transceiver");
            // Start listening
            messenger.StartListening();

            SendCommand(Command.Initialize, "Initialize");
            running = true;
        }

        public void DeInitialize()
        {
            logger.LogInfo("Stopping serial command transceiver");

            SendCommand(Command.Initialize, "DeInitialize");

            messenger.StopListening();
            running = false;
        }

        #endregion

        #region IDisposable Implementation

        bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Clean up managed objects
                    DeInitialize();
                    
                    if (messenger != null)
                    {
                        messenger.Dispose();
                        messenger = null;
                    }
                    if (transport != null)
                    {
                        transport.Dispose();
                        transport = null;
                    }
                }
                // clean up any unmanaged objects
                disposed = true;
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
            messenger.Attach(OnReceivedCommand);
            messenger.Attach((int)Command.Acknowledge, OnAcknowledge);
            messenger.Attach((int)Command.Error, OnError);
        }

        // Called when a received command has no internally attached function.
        void OnReceivedCommand(ReceivedCommand arguments)
        {
            try
            {
                var command = (Command)arguments.CmdId;
                if (subscriptions.Contains(command))
                {
                   
                    logger.LogDebug(String.Format(CultureInfo.InvariantCulture, "Received Command: {0}",
                            command));

                    CommandReceived.Invoke(this, new CommandReceivedEventArgs(command));

                }
                else
                {
                    logger.LogWarning("Received command without subscribers: " + command);                    
                }
            }
            catch (InvalidCastException)
            {
                logger.LogWarning("Received unsupported command: "+arguments.rawString);
            }
        }

        #region ICommandTransmitter Members

        public void SendCommand(Command commandType, string context)
        {
            // Create command FloatAddition, which will wait for a return command FloatAdditionResult
            var command = new SendCommand((int)commandType, context);

            // Send command
            messenger.SendCommand(command);

            logger.LogDebug(String.Format(CultureInfo.InvariantCulture, "Sent Command: {0}",
                                         commandType));
        }

        #endregion

        // Callback function that prints that the Arduino has acknowledged
        private void OnAcknowledge(ReceivedCommand arguments)
        {
            logger.LogDebug("Acknowledged: " + arguments.CmdId);
        }

        // Callback function that prints that the Arduino has experienced an error
        private void OnError(ReceivedCommand arguments)
        {
            logger.LogError("Remote side has experienced an error");
        }

        // Log received line to console
        private void NewLineReceived(object sender, EventArgs e)
        {
            logger.LogDebug("Received > " + messenger.CurrentReceivedLine);
        }

        // Log sent line to console
        private void NewLineSent(object sender, EventArgs e)
        {
            logger.LogDebug("Sent > " + messenger.CurrentSentLine);
        }
    }
}
