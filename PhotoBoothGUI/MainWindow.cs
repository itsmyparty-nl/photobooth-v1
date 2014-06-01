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

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		
		//Initialize the photobooth
		//Instantiate all classes
		logger = new TextBoxLogger (textview1.Buffer);
		ISettings settings = new com.prodg.photobooth.config.Settings (logger);


		//var camera = new CameraStub (logger);
		var camera = new Camera (logger);
		var printer = new NetPrinter(settings, logger);
		//var printer = new PrinterStub (logger);
		var commandMessenger = new CommandMessengerTransceiver(logger, settings);
		var triggerControl = new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
		var printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
        var printTwiceControl = new RemoteTrigger(Command.PrintTwice, commandMessenger, commandMessenger, logger);
        var powerControl = new RemoteTrigger(Command.Power, commandMessenger, commandMessenger, logger);

		//var triggerControl = new ButtonRemoteControl (Command.Trigger.ToString (), buttonTrigger);
		//var printControl = new ButtonRemoteControl (Command.Print.ToString (), buttonPrint);
		//var powerControl = new ButtonRemoteControl (Command.Power.ToString (), buttonExit);
        
		hardware = new Hardware (camera, printer, triggerControl, printControl, printTwiceControl, powerControl, logger);

		IImageProcessor imageProcessor = new CollageImageProcessor (logger, settings);
		IPhotoBoothService photoService = new PhotoBoothService(hardware, imageProcessor, logger, settings);
		photoBooth = new PhotoBoothModel (photoService, hardware, logger);

		//Subscribe to the shutdown requested event 
		photoBooth.ShutdownRequested += OnPhotoBoothShutdownRequested; 
		photoService.PictureAdded += PhotoBoothServiceOnPictureAdded;

		//Start
		photoBooth.Start ();
        
		statusbar1.Push (1, hardware.Camera.Id);
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

	private void Stop ()
	{
		photoBooth.Stop ();
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
