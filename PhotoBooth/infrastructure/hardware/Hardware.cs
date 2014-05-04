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

using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
    public class Hardware : IHardware
    {
        private readonly ILogger logger;

		public Hardware(ICamera camera, IPrinter printer, ITriggerControl triggerControl, ITriggerControl printControl,
            ITriggerControl powerControl, ILogger logger)
        {
            Camera = camera;
			Printer = printer;
            TriggerControl = triggerControl;
            PrintControl = printControl;
            PowerControl = powerControl;
            this.logger = logger;
        }

        #region IHardware Members

        public ICamera Camera { get; private set; }

		public IPrinter Printer { get; private set; }

        public ITriggerControl TriggerControl { get; private set; }

        public ITriggerControl PrintControl { get; private set; }

        public ITriggerControl PowerControl { get; private set; }

        public void Acquire()
        {
            logger.LogInfo("Initializing hardware");

            Camera.Initialize();
			Printer.Initialize ();
            TriggerControl.Initialize();
            PrintControl.Initialize();
            PowerControl.Initialize();
        }

        public void Release()
        {
            logger.LogInfo("Releasing hardware");

            TriggerControl.ReleaseTrigger();
            PowerControl.ReleaseTrigger();
            PrintControl.ReleaseTrigger();

            Camera.DeInitialize();
			Printer.DeInitialize ();
			TriggerControl.DeInitialize();
            PrintControl.DeInitialize();
            PowerControl.DeInitialize();
        }

        #endregion
    }
}
