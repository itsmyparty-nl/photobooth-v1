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
using Gtk;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.command;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class GuiHardware: IHardware
	{
		private readonly ILogger logger;

		public GuiHardware(ILogger logger, Button triggerButton, Button printButton, Button powerButton)
		{
			this.logger = logger;
			Camera = new Camera(logger);

			TriggerControl = new ButtonRemoteControl(Command.Trigger.ToString(), triggerButton);
			PrintControl = new ButtonRemoteControl(Command.Print.ToString(), printButton);
			PowerControl = new ButtonRemoteControl(Command.Power.ToString(), powerButton);
		}

		#region IHardware Members

		public ICamera Camera { get; private set; }

		public IRemoteControl TriggerControl { get; private set; }

		public IRemoteControl PrintControl { get; private set; }

		public IRemoteControl PowerControl { get; private set; }

		public void Initialize()
		{
			logger.LogInfo("Initializing hardware v1");

			Camera.Initialize();
		}

		public void Release()
		{
			logger.LogInfo("Releasing hardware v1");

			TriggerControl.Release();
			PowerControl.Release();
			PrintControl.Release();
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
				}
				// clean up any unmanaged objects
				disposed = true;
			}
			else
			{
				Console.WriteLine("Saved us from doubly disposing an object!");
			}
		}

		~GuiHardware()
		{
			Dispose(false);
		}

		#endregion
	}
}
