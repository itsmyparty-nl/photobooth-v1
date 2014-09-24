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
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using Gtk;
using Pango;
using System.Collections.Generic;
using com.prodg.photobooth.config;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.common;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;


public partial class MainWindow: Gtk.Window
{
	private ILogger logger;
	private IHardware hardware;
	private ICamera camera;
	private IPhotoBoothModel photoBooth;
	private CommandMessengerTransceiver commandMessenger;
	private Gdk.Cursor invisibleCursor;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		//Instantiate all classes
		logger = new NLogger ();

		logger.LogInfo ("Creating photobooth GUI application");
		//Register all unhandled exception handlers
		AppDomain.CurrentDomain.UnhandledException += HandleUnhandledAppdomainException;
		GLib.ExceptionManager.UnhandledException += HandleUnhandledGlibException;
		TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

		//Initialize
		ISettings settings = new com.prodg.photobooth.config.Settings (logger);

		PreloadImages ();
		HideCursor ();

		camera = new Camera (logger);
		commandMessenger = new CommandMessengerTransceiver (logger, settings);

		camera.StateChanged += OnCameraStateChanged;
		camera.BatteryWarning += OnCameraBatteryWarning;
		//var printer = new NetPrinter (settings, logger);
		var printer = new PrinterStub(logger);
                var triggerControl = new RemoteTrigger (Command.Trigger, commandMessenger, commandMessenger, logger);
		var printControl = new RemoteTrigger (Command.Print, commandMessenger, commandMessenger, logger);
		var printTwiceControl = new RemoteTrigger (Command.PrintTwice, commandMessenger, commandMessenger, logger);
        //var printControl = new TriggerControlStub(Command.Print.ToString(), 300, logger);
        //var printTwiceControl = new TriggerControlStub(Command.PrintTwice.ToString(), null, logger);
		
        var powerControl = new RemoteTrigger (Command.Power, commandMessenger, commandMessenger, logger);
		hardware = new Hardware (camera, printer, triggerControl, printControl, printTwiceControl,
			                      powerControl, logger);

		IImageProcessor imageProcessor = new CollageImageProcessor (logger, settings);
		IPhotoBoothService photoBoothService = new PhotoBoothService (hardware, imageProcessor, logger, settings);
		photoBooth = new PhotoBoothModel (photoBoothService, hardware, logger);


		//Subscribe to the shutdown requested event 
		photoBooth.ShutdownRequested += OnPhotoBoothShutdownRequested; 
		photoBoothService.PictureAdded += OnPhotoBoothServicePictureAdded;
        printControl.Fired += OnPrintControlFired;
        printTwiceControl.Fired += OnPrintTwiceControlFired;
        triggerControl.Fired += OnTriggerControlFired;
		photoBooth.ErrorOccurred += OnPhotoBoothErrorOccurred;

		statusbar1.Push (1, "Waiting for camera");

		imagePhoto.Pixbuf = instructionImages ["instruction"];
		imageInstruction.Pixbuf = instructionImages ["title"];

		Start ();

		//textview1.Visible = false;
		//GtkScrolledWindow.Visible = false;
		this.Fullscreen ();
	}

	void OnPhotoBoothErrorOccurred (object sender, com.prodg.photobooth.domain.ErrorEventArgs e)
	{
		Gtk.Application.Invoke ((b, c) => {
			imagePhoto.Pixbuf = instructionImages ["error"];
			imageInstruction.Pixbuf = instructionImages ["title"];
		});
	}

	private Dictionary<string, Gdk.Pixbuf> instructionImages;

	private void HideCursor()
	{
		using (Gdk.Pixmap inv = new Gdk.Pixmap (null, 1, 1, 1)) {
			invisibleCursor = new Gdk.Cursor (inv, inv, Gdk.Color.Zero,
				Gdk.Color.Zero, 0, 0);
		}
		GdkWindow.Cursor = invisibleCursor;
	}

	private void ShowCursor()
	{
		if (invisibleCursor != null)
		{
			invisibleCursor.Dispose ();
			invisibleCursor = null;
		}
	}

	private void PreloadImages()
	{
		instructionImages = new Dictionary<string, Gdk.Pixbuf> ();
		var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly ().GetName ().CodeBase).Replace("file:","");
		var resources = System.IO.Path.Combine(path, "Resources" );
		instructionImages.Add("title",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"tilte.png")));
		instructionImages.Add("indicator_1",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"indicator1of4.png")));
		instructionImages.Add("indicator_2",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"indicator2of4.png")));
		instructionImages.Add("indicator_3",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"indicator3of4.png")));
		instructionImages.Add("indicator_4",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"indicator4of4.png")));
		instructionImages.Add("instruction",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"largeinstruction.png")));
		instructionImages.Add("finished",new Gdk.Pixbuf (System.IO.Path.Combine(resources,"legend.png")));
		instructionImages.Add ("ready", new Gdk.Pixbuf (System.IO.Path.Combine (resources, "ready.png")));
		instructionImages.Add ("empty", new Gdk.Pixbuf (System.IO.Path.Combine (resources, "empty.png")));
		instructionImages.Add ("error", new Gdk.Pixbuf (System.IO.Path.Combine (resources, "error.png")));
	}

    private void OnPrintControlFired(object sender, TriggerControlEventArgs e)
    {
		Gtk.Application.Invoke ((b, c) => {
			imagePhoto.Pixbuf = instructionImages ["instruction"];
			imageInstruction.Pixbuf = instructionImages ["title"];
		});
    }

    private void OnPrintTwiceControlFired(object sender, TriggerControlEventArgs e)
    {
		Gtk.Application.Invoke ((b, c) => {
			imagePhoto.Pixbuf = instructionImages ["instruction"];
			imageInstruction.Pixbuf = instructionImages ["title"];
		});
    }

    private void OnTriggerControlFired(object sender, TriggerControlEventArgs e)
    {
		Gtk.Application.Invoke ((b, c) => {
			imageInstruction.Pixbuf = instructionImages ["ready"];
			imagePhoto.Pixbuf = instructionImages ["empty"];
		});
    }

	private void Start()
	{
		logger.LogInfo ("PhotoboothGUI.Start()");
		//Start
		commandMessenger.Initialize ();
		photoBooth.Start ();
	}

	private void Stop ()
	{
		logger.LogInfo ("PhotoboothGUI.Stop()");
		//Stop
		photoBooth.Stop();
		commandMessenger.DeInitialize();
	}

	private void Shutdown()
	{	
		logger.LogInfo ("PhotoboothGUI.Shutdown()");
		Stop ();
		ShowCursor ();

		logger.LogInfo ("Disposing hardware");
		if (photoBooth != null) {
			photoBooth.Dispose ();
			photoBooth = null;
		}
		if (commandMessenger != null) {
			commandMessenger.Dispose ();
			commandMessenger = null;
		}
		if (camera != null) {
			camera.Dispose ();
			camera = null;
		}

		logger.LogInfo ("Quitting application");
		Application.Quit ();
	}

	void HandleUnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs e)
	{
		if (HandleUnhandledException (e.Exception, true)) {
			e.SetObserved ();
		}
	}

	void HandleUnhandledGlibException (GLib.UnhandledExceptionArgs args)
	{
		if (HandleUnhandledException ((Exception)args.ExceptionObject, true)) {
			args.ExitApplication = false;
		}
	}

	void HandleUnhandledAppdomainException (object sender, UnhandledExceptionEventArgs e)
	{
		HandleUnhandledException((Exception)e.ExceptionObject, false);	
	}

	private bool HandleUnhandledException(Exception exception, bool tryToRecover)
	{
		logger.LogException ("Caught unhandled exception", exception);
		if (tryToRecover) {
			try {
				imagePhoto.Pixbuf = instructionImages["error"];
				Stop ();
				Start ();
				return true;
			} catch (Exception) {
				logger.LogException ("Unable to restore application, quitting", exception);
				return false;
			}
		}
		return false;
	}

	private void OnPhotoBoothServicePictureAdded (object sender, PictureAddedEventArgs a)
	{
		Gtk.Application.Invoke ((b, c) => {
			if (a.IsFinal) {
				imageInstruction.Pixbuf = instructionImages ["finished"];
			} else {
				imageInstruction.Pixbuf = instructionImages ["indicator_" + (a.Index + 1)];
			}
			//Create and scale the pixbuf
			var result = CreateAndScalePicture (a.Picture, imagePhoto.Allocation.Height);

			//Set the pixbuf on the UI
			imagePhoto.Pixbuf = result.Result;
			ShowAll ();
		});
	}

	private async Task<Gdk.Pixbuf> CreateAndScalePicture(System.Drawing.Image picture, int height)
    {
		return await Task.Run(() =>
	    {
	        using (var stream = new MemoryStream())
	        {
	            picture.Save(stream, ImageFormat.Bmp);
	            stream.Position = 0;
                var pixBuf = new Gdk.Pixbuf(stream);

	            double scale = height/(double) pixBuf.Height;
                return pixBuf.ScaleSimple((int)(scale * pixBuf.Width), (int)(scale * pixBuf.Height), Gdk.InterpType.Bilinear);
	        }
	    });
	}

	void OnPhotoBoothShutdownRequested (object sender, EventArgs e)
	{
		Gtk.Application.Invoke ((b, c) => {
			Shutdown ();
		});
	}

	void OnCameraBatteryWarning(object sender, CameraBatteryWarningEventArgs e)
	{
		Gtk.Application.Invoke ((b, c) => {
			string logString = string.Format (CultureInfo.InvariantCulture, "WARNING BATTERY LOW: {0}%", e.Level);
			logger.LogWarning (logString);
			statusbar1.Push (1, logString);
		});
	}

	void OnCameraStateChanged(object sender, CameraStateChangedEventArgs e)
	{
		if (e.NewState)
		{
			Gtk.Application.Invoke ((b, c) => {
				statusbar1.Push (1, hardware.Camera.Id);
			});
		}
		else
		{
			Gtk.Application.Invoke ((b, c) => {
				statusbar1.Push (1, "Check camera connection");
			});
		}
	}
		
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Shutdown ();
		a.RetVal = true;
	}

	protected void OnKeyPress (object o, KeyPressEventArgs args)
	{
		this.Unfullscreen ();
	}
}
