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
using System.IO.Ports;
using System.Threading;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.command
{
    public class SerialCommandTransceiver : ICommandReceiver, ICommandTransmitter
    {
        private readonly ILogger logger;
        private bool running;
        private Thread listenerThread;

        private SerialPort serialPort;

        private const string DefaultPortName = "COM1";
        private const int DefaultBaudRate = 115200;
        private const Parity DefaultParity = Parity.None;

        private Queue<string> commandQueue;
        private readonly List<Command> subscriptions = new List<Command>();


        public SerialCommandTransceiver(ILogger logger)
        {
            this.logger = logger;

            //serialPort = new SerialPort(DefaultPortName, DefaultBaudRate, DefaultParity)
            //    {
            //        Handshake = Handshake.None,
            //        // Set the read/write timeouts
            //        ReadTimeout = 100,
            //        WriteTimeout = 100
            //    };


            commandQueue = new Queue<string>();
        }

        #region ICommandReceiver Members

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        public void Subscribe(Command command)
        {
            subscriptions.Add(command);
        }

        #endregion

        #region IHardwareController Members

        public void Start()
        {
            if (running || listenerThread != null)
            {
                throw new InvalidOperationException("Not allowed to start twice");
            }
            listenerThread = new Thread(SerialWorker);
            running = true;
            commandQueue.Clear();

            logger.LogInfo("Starting serial command transceiver");
            //serialPort.Open();

            listenerThread.Start();
            running = true;
        }

        public void Stop()
        {
            logger.LogInfo("Stopping serial command transceiver");

            running = false;
            if (!listenerThread.Join(5000))
            {
                logger.LogWarning("Serial listener not stopped on time: force quit");
                listenerThread.Abort();
            }

            //serialPort.Close();
        }

        private void SerialWorker()
        {
            while (running)
            {
                //Process commands
                string message = null; // serialPort.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    //Parse & Handle command
                    HandleCommand(message);
                }

                //Trigger commands
                while (commandQueue.Count > 0)
                {
                    if (!string.IsNullOrEmpty(commandQueue.Peek()))
                    {
                        logger.LogInfo("Sending serial data: " + commandQueue.Dequeue());
                        //serialPort.WriteLine(commandQueue.Dequeue());
                    }
                }
            }
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
                    Stop();

                    if (serialPort != null)
                    {
                        serialPort.Dispose();
                        serialPort = null;
                    }
                    if (commandQueue != null)
                    {
                        commandQueue.Clear();
                        commandQueue = null;
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

        ~SerialCommandTransceiver()
        {
            Dispose(false);
        }

        #endregion

        #region ICommandTransmitter Members

        public void SendCommand(Command buttonType, string context, string value)
        {
            string compiledCommand = CompileCommand(buttonType, context, value);
            logger.LogInfo(String.Format(CultureInfo.InvariantCulture, "Send Command: {0}",
                                         compiledCommand));

            commandQueue.Enqueue(compiledCommand);
        }

        private string CompileCommand(Command buttonType, string context, string value)
        {
            return String.Format(CultureInfo.InvariantCulture, "C:{0}:{1}:{2}", buttonType, context, value);
        }

        private void HandleCommand(string serialLine)
        {
            string[] parts = serialLine.Split(':');
            if (parts.Length != 4 || !parts[0].Equals("C"))
            {
                return;
            }
            var commandType = (Command)Enum.Parse(typeof(Command), parts[1]);
            string context = parts[2];
            string value = parts[3];

            logger.LogInfo(String.Format(CultureInfo.InvariantCulture, "Received command:{0} - {1} - {2}", commandType,
                                         context, value));

            if (subscriptions.Contains(commandType) && CommandReceived != null)
            {
                //Throw the received event
                CommandReceived.Invoke(this, new CommandReceivedEventArgs(commandType));
            }
        }


        #endregion
    }
}
