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

using com.prodg.photobooth.infrastructure.command;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class Hardware : IHardware
    {
        private readonly ILogger<Hardware> _logger;

		public Hardware(ICamera camera, IPrinter printer, ILogger<Hardware> logger)
        {
            Camera = camera;
			Printer = printer;
            
            TriggerControl = new TriggerControl(Command.Trigger);
            PrintControl = new TriggerControl(Command.Print);
            PrintTwiceControl = new TriggerControl(Command.PrintTwice);
            PowerControl = new TriggerControl(Command.Power);
            _logger = logger;
        }

        #region IHardware Members

        public ICamera Camera { get; private set; }

		public IPrinter Printer { get; private set; }

        public ITriggerControl TriggerControl { get; private set; }

        public ITriggerControl PrintControl { get; private set; }

        public ITriggerControl PrintTwiceControl { get; private set; }

        public ITriggerControl PowerControl { get; private set; }

        public void Acquire()
        {
            _logger.LogInformation("Initializing hardware");

            Camera.Initialize();
			Printer.Initialize ();
            TriggerControl.Initialize();
            PrintControl.Initialize();
            PrintTwiceControl.Initialize();
            PowerControl.Initialize();
        }

        public void Release()
        {
            _logger.LogInformation("Releasing hardware");

            TriggerControl.Release();
            PowerControl.Release();
            PrintControl.Release();
            PrintTwiceControl.Release();

            Camera.DeInitialize();
			Printer.DeInitialize ();
			TriggerControl.DeInitialize();
            PrintControl.DeInitialize();
            PrintTwiceControl.DeInitialize();
            PowerControl.DeInitialize();
        }

        #endregion
    }
}
