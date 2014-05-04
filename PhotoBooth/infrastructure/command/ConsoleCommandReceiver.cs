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
using System.Linq;
using System.Threading;
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.command
{
    public class ConsoleCommandReceiver : ICommandReceiver
    {
        private const string Id = "ConsoleCommandReceiver";

        private readonly ILogger logger;
        private readonly Dictionary<Command, ConsoleKey> commandKeyMapping;
        private readonly List<ConsoleKey> subscriptions = new List<ConsoleKey>();
        private Thread listenerThread;
        private bool running;

        public ConsoleCommandReceiver(ILogger logger)
        {
            this.logger = logger;
            running = false;

            //Create a mapping between console keys and remote control buttons
            commandKeyMapping = new Dictionary<Command, ConsoleKey>
                {
                    {Command.Trigger, ConsoleKey.T},
                    {Command.Print, ConsoleKey.P},
                    {Command.Power, ConsoleKey.Q}
                };
        }

        #region ICommandReceiver Members

        public void Initialize()
        {
            if (running || listenerThread != null)
            {
                throw new InvalidOperationException("Not allowed to start twice");
            }
            listenerThread = new Thread(ListenToConsoleWorker);
            running = true;
            listenerThread.Start();
            logger.LogInfo(String.Format(CultureInfo.InvariantCulture, "{0}: Started", Id));
        }

        public void DeInitialize()
        {
            if (running && listenerThread != null)
            {
                running = false;
                listenerThread.Join();
            }
            logger.LogInfo(String.Format(CultureInfo.InvariantCulture, "{0}: Stopped", Id));
            listenerThread = null;
        }

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        public void Subscribe(Command command)
        {
            if (!commandKeyMapping.ContainsKey(command))
            {
                throw new NotSupportedException("The provided command is not supported: " + command);
            }
            if (!subscriptions.Contains(commandKeyMapping[command]))
            {
                subscriptions.Add(commandKeyMapping[command]);
            }
        }

        public void UnSubscribe(Command command)
        {
            if (!commandKeyMapping.ContainsKey(command))
            {
                throw new NotSupportedException("The provided command is not supported: " + command);
            }
            if (subscriptions.Contains(commandKeyMapping[command]))
            {
                subscriptions.Remove(commandKeyMapping[command]);
            }            
        }

        #endregion

        private void ListenToConsoleWorker()
        {
            while (running)
            {
                while (!Console.KeyAvailable && running)
                {
                    Thread.Sleep(50);
                }
                if (running)
                {
                    var consoleKeyInfo = Console.ReadKey();
                    if (subscriptions.Contains(consoleKeyInfo.Key))
                    {
                        KeyValuePair<Command, ConsoleKey> commandKey = commandKeyMapping.First(kvp => kvp.Value == consoleKeyInfo.Key);
                        logger.LogInfo(String.Format(CultureInfo.InvariantCulture, "{0}: {1} command received", Id,
                                                     commandKey.Key.ToString()));

                        //Only trigger if we have listeners
                        if (CommandReceived != null)
                        {
                            CommandReceived.Invoke(this, new CommandReceivedEventArgs(commandKey.Key));
                        }
                    }
                }
            }
        }

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
                }
                // clean up any unmanaged objects
                disposed = true;
            }
            else
            {
                Console.WriteLine("Saved us from doubly disposing an object!");
            }
        }


        ~ConsoleCommandReceiver()
        {
            Dispose(false);
        }

        #endregion
    }
}
