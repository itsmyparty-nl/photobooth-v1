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
using com.prodg.photobooth.config;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.common;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;


public partial class MainWindow: Gtk.Window
{
	private ILogger logger;
	private IHardware hardware;
	private readonly IPhotoBoothModel photoBooth;
	private CommandMessengerTransceiver commandMessenger;
	private ConsoleCommandReceiver consoleCommandReceiver;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		//Instantiate all classes
		logger = new NLogger ();
		ISettings settings = new com.prodg.photobooth.config.Settings (logger);

		var camera = new Camera (logger);
		var commandMessenger = new CommandMessengerTransceiver (logger, settings);
		var consoleCommandReceiver = new ConsoleCommandReceiver (logger);

		camera.StateChanged += OnCameraStateChanged;
		camera.BatteryWarning += OnCameraBatteryWarning;
		var printer = new NetPrinter (settings, logger);
		var triggerControl = new RemoteTrigger (Command.Trigger, commandMessenger, commandMessenger, logger);
		var printControl = new RemoteTrigger (Command.Print, commandMessenger, commandMessenger, logger);
		var printTwiceControl = new RemoteTrigger (Command.PrintTwice, commandMessenger, commandMessenger, logger);
		var powerControl = new RemoteTrigger (Command.Power, commandMessenger, commandMessenger, logger);
		hardware = new Hardware (camera, printer, triggerControl, printControl, printTwiceControl,
			                      powerControl, logger);

		IImageProcessor imageProcessor = new CollageImageProcessor (logger, settings);
		IPhotoBoothService photoBoothService = new PhotoBoothService (hardware, imageProcessor, logger, settings);
		IPhotoBoothModel photoBooth = new PhotoBoothModel (photoBoothService, hardware, logger);

		//Subscribe to the shutdown requested event 
		photoBooth.ShutdownRequested += OnPhotoBoothShutdownRequested; 
		photoBoothService.PictureAdded += PhotoBoothServiceOnPictureAdded;

		statusbar1.Push (1, "Waiting for camera");

		//Start
		commandMessenger.Initialize ();
		consoleCommandReceiver.Initialize ();
		photoBooth.Start ();

		//textview1.Visible = false;
		//GtkScrolledWindow.Visible = false;
		this.Fullscreen ();
	}

	private void PhotoBoothServiceOnPictureAdded (object sender, PictureAddedEventArgs a)
	{
		Gtk.Application.Invoke((b,c) =>
	    {
	        //Create and scale the pixbuf
			var result = CreateAndScalePicture(a.Picture, imagePhoto.Allocation.Width);

	        //Set the pixbuf on the UI
			imagePhoto.Pixbuf = result.Result;
	        ShowAll();
	    });
	}

	private async Task<Gdk.Pixbuf> CreateAndScalePicture(System.Drawing.Image picture, int width)
    {
		return await Task.Run(() =>
	    {
	        using (var stream = new MemoryStream())
	        {
	            picture.Save(stream, ImageFormat.Bmp);
	            stream.Position = 0;
                var pixBuf = new Gdk.Pixbuf(stream);

	            double scale = width/(double) pixBuf.Width;
                return pixBuf.ScaleSimple((int)(scale * pixBuf.Width), (int)(scale * pixBuf.Height), Gdk.InterpType.Bilinear);
	        }
	    });
	}

	void OnPhotoBoothShutdownRequested (object sender, EventArgs e)
	{
		Gtk.Application.Invoke ((b, c) => {
			Stop ();
		});
	}

	void OnCameraBatteryWarning(object sender, CameraBatteryWarningEventArgs e)
	{
		string logString = string.Format (CultureInfo.InvariantCulture, "WARNING BATTERY LOW: {0}%", e.Level);
		logger.LogWarning(logString);
		statusbar1.Push (1, logString);
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

	private void Stop ()
	{
		//Stop
		photoBooth.Stop();
		consoleCommandReceiver.DeInitialize();
		commandMessenger.DeInitialize();
		Application.Quit ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Stop ();
		a.RetVal = true;
	}

	protected void OnKeyPress (object o, KeyPressEventArgs args)
	{
		this.Unfullscreen ();
	}
}
