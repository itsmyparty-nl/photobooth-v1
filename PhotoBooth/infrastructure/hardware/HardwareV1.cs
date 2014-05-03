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
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class HardwareV1: IHardware
    {
        private ICommandReceiver commandReceiver;
        private ICommandTransmitter commandTransmitter;
        private readonly ILogger logger;
        
        public HardwareV1(ILogger logger)
        {
            this.logger = logger;
            Camera = new Camera(logger);
            //Camera = new CameraStub(logger);

            commandReceiver = new ConsoleCommandReceiver(logger);
            commandTransmitter = new SerialCommandTransceiver(logger);

            TriggerControl = new RemoteTrigger(Command.Trigger, commandReceiver, commandTransmitter, logger);
            PrintControl = new RemoteTrigger(Command.Print, commandReceiver, commandTransmitter, logger);
            PowerControl = new RemoteTrigger(Command.Power, commandReceiver, commandTransmitter, logger);
        }
        
        #region IHardware Members

        public ICamera Camera { get; private set; }

        public ITriggerControl TriggerControl { get; private set; }
    
        public ITriggerControl PrintControl { get; private set; }

        public ITriggerControl PowerControl { get; private set; }
       
        public void Acquire()
        {
            logger.LogInfo("Initializing hardware v1");

            Camera.Initialize();

            commandReceiver.Start();
            commandTransmitter.Start();

        }

        public void Release()
        {
            logger.LogInfo("Releasing hardware v1");
            
            TriggerControl.Release();
            PowerControl.Release();
            PrintControl.Release();

            commandReceiver.Stop();
            commandTransmitter.Stop();
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
                    if (Camera != null)
                    {
                        try
                        {
                            Camera.Dispose();
                        }
                        catch (HardwareException ex)
                        {
                           logger.LogException("Could not deinitialize camera", ex);
                        }
                        Camera = null;
                    }
                    if (TriggerControl != null)
                    {
                        TriggerControl.Dispose();
                        TriggerControl = null;
                    }
                    if (PowerControl != null)
                    {
                        PowerControl.Dispose();
                        PowerControl = null;
                    }
                    if (PrintControl != null)
                    {
                        PrintControl.Dispose();
                        PrintControl = null;
                    }
                    if (commandReceiver != null)
                    {
                        commandReceiver.Dispose();
                        commandReceiver = null;
                    }
                    if (commandTransmitter != null)
                    {
                        commandTransmitter.Dispose();
                        commandTransmitter = null;
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

        ~HardwareV1()
        {
            Dispose(false);
        }

        #endregion
    }
}
