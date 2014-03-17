/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

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
        private readonly Dictionary<CommandType, ConsoleKey> commandKeyMapping;
        private readonly List<ConsoleKey> subscriptions = new List<ConsoleKey>();
        private Thread listenerThread;
        private bool running;

        public ConsoleCommandReceiver(ILogger logger)
        {
            this.logger = logger;
            running = false;

            //Create a mapping between console keys and remote control buttons
            commandKeyMapping = new Dictionary<CommandType, ConsoleKey>
                {
                    {CommandType.Trigger, ConsoleKey.T},
                    {CommandType.Print, ConsoleKey.P},
                    {CommandType.Power, ConsoleKey.Q}
                };
        }

        #region ICommandReceiver Members

        public void Start()
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

        public void Stop()
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

        public void Subscribe(CommandType command)
        {
            if (!commandKeyMapping.ContainsKey(command))
            {
                throw new NotSupportedException("The provided command is not supported: " + command.ToString());
            }
            subscriptions.Add(commandKeyMapping[command]);
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
                        KeyValuePair<CommandType, ConsoleKey> commandKey = commandKeyMapping.First(kvp => kvp.Value == consoleKeyInfo.Key);
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
                    Stop();
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
