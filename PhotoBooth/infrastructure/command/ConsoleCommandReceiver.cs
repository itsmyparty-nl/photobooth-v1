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

using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.command
{
    public class ConsoleCommandReceiver : IConsoleCommandReceiver
    {
        private const string Id = "ConsoleCommandReceiver";

        private readonly ILogger<ConsoleCommandReceiver> _logger;
        private readonly Dictionary<Command, ConsoleKey> _commandKeyMapping;
        private readonly List<ConsoleKey> _subscriptions = new();
        private Thread? _listenerThread;
        private bool _running;

        public ConsoleCommandReceiver(ILogger<ConsoleCommandReceiver> logger)
        {
            _logger = logger;
            _running = false;

            //Create a mapping between console keys and remote control buttons
            _commandKeyMapping = new Dictionary<Command, ConsoleKey>
                {
                    {Command.Trigger, ConsoleKey.T},
                    {Command.Print, ConsoleKey.NumPad1},
                    {Command.PrintTwice, ConsoleKey.NumPad2},
                    {Command.Power, ConsoleKey.Q}
                };
        }

        #region ICommandReceiver Members

        public void Initialize()
        {
            if (_running || _listenerThread != null)
            {
                throw new InvalidOperationException("Not allowed to start twice");
            }
            _listenerThread = new Thread(ListenToConsoleWorker);
            _running = true;
            _listenerThread.Start();
            _logger.LogInformation("{Id}: Started", Id);
        }

        public void DeInitialize()
        {
            if (_running && _listenerThread != null)
            {
                _running = false;
                _listenerThread.Join();
            }
            _logger.LogInformation("{Id}: Stopped", Id);
            _listenerThread = null;
        }

        public event EventHandler<CommandReceivedEventArgs>? CommandReceived;

        public void Subscribe(Command command)
        {
            if (!_commandKeyMapping.ContainsKey(command))
            {
                throw new NotSupportedException("The provided command is not supported: " + command);
            }
            if (!_subscriptions.Contains(_commandKeyMapping[command]))
            {
                _subscriptions.Add(_commandKeyMapping[command]);
            }
        }

        public void UnSubscribe(Command command)
        {
            if (!_commandKeyMapping.ContainsKey(command))
            {
                throw new NotSupportedException("The provided command is not supported: " + command);
            }
            if (_subscriptions.Contains(_commandKeyMapping[command]))
            {
                _subscriptions.Remove(_commandKeyMapping[command]);
            }            
        }

        #endregion

        private void ListenToConsoleWorker()
        {
            while (_running)
            {
                while (!Console.KeyAvailable && _running)
                {
                    Thread.Sleep(50);
                }
                if (_running)
                {
                    var consoleKeyInfo = Console.ReadKey();
                    if (_subscriptions.Contains(consoleKeyInfo.Key))
                    {
                        KeyValuePair<Command, ConsoleKey> commandKey =
                            _commandKeyMapping.First(kvp => kvp.Value == consoleKeyInfo.Key);
                        _logger.LogInformation("{Id}: {CommandKey} command received", Id, commandKey.Key);

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
                }
                // clean up any unmanaged objects
                _disposed = true;
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
