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
using System.IO;
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


		var camera = new Camera (logger);
		//var commandMessenger = new CommandMessengerTransceiver(logger, settings);
		//var triggerControl = new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
		//var printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
		//var powerControl = new RemoteTrigger(Command.Power, commandMessenger, commandMessenger, logger);

		var triggerControl = new ButtonRemoteControl (Command.Trigger.ToString (), buttonTrigger);
		var printControl = new ButtonRemoteControl (Command.Print.ToString (), buttonPrint);
		var powerControl = new ButtonRemoteControl (Command.Power.ToString (), buttonExit);
        
		hardware = new Hardware (camera, triggerControl, printControl, powerControl, logger);

		IImageProcessor imageProcessor = new CollageImageProcessor (logger, settings);
		IPhotographyService photoService = new PhotographyService (hardware, imageProcessor, logger, settings);
		IPrintingService printingService = new GdkPrintingService (settings,logger);
		photoBooth = new PhotoBoothModel (photoService, printingService, hardware, logger);

		//Subscribe to the shutdown requested event 
		photoBooth.ShutdownRequested += OnPhotoBoothShutdownRequested; 
		photoService.PictureAdded += PhotoBoothServiceOnPictureAdded;

		//Start
		photoBooth.Start ();
        
		statusbar1.Push (1, hardware.Camera.Id);
		//textview1.Visible = false;
		this.Fullscreen ();
	}

	private void PhotoBoothServiceOnPictureAdded (object sender, PictureAddedEventArgs a)
	{
		//Dispose any previous image in the buffer
		//if (imagePhoto.Pixbuf != null)
		//{
		//    var pixBuf = imagePhoto.Pixbuf;
		//    imagePhoto.Pixbuf = null;
		//    pixBuf.Dispose();
		//}

		//Set the new image
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) { 
			a.Picture.Save (stream, System.Drawing.Imaging.ImageFormat.Bmp); 
			stream.Position = 0; 
			imagePhoto.Pixbuf = new Gdk.Pixbuf (stream); 
		} 
		//ShowAll();
		//imagePhoto.Show ();
	}

	void OnPhotoBoothShutdownRequested (object sender, System.EventArgs e)
	{
		Stop ();
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
