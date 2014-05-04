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
        ISettings settings = new Settings();
        logger = new TextBoxLogger(textview1.Buffer);

        var camera = new CameraStub(logger);
        var commandMessenger = new CommandMessengerTransceiver(logger, settings);
        //var triggerControl = new RemoteTrigger(Command.Trigger, commandMessenger, commandMessenger, logger);
        //var printControl = new RemoteTrigger(Command.Print, commandMessenger, commandMessenger, logger);
        //var powerControl = new RemoteTrigger(Command.Power, commandMessenger, commandMessenger, logger);

        var triggerControl = new ButtonRemoteControl(Command.Trigger.ToString(), buttonTrigger);
        var printControl = new ButtonRemoteControl(Command.Print.ToString(), buttonPrint);
        var powerControl = new ButtonRemoteControl(Command.Power.ToString(), buttonExit);
        
        hardware = new Hardware(camera, triggerControl, printControl, powerControl, logger);

        IImageProcessor imageProcessor = new CollageImageProcessor(logger, settings);
        IPhotoBoothService photoBoothService = new PhotoBoothService(hardware, imageProcessor, logger, settings);
        photoBooth = new PhotoBoothModel(photoBoothService, hardware, logger);

        //Subscribe to the shutdown requested event 
        photoBooth.ShutdownRequested += OnPhotoBoothShutdownRequested; 
        photoBoothService.PictureAdded += PhotoBoothServiceOnPictureAdded;

        //Start
        photoBooth.Start();
        
		//Start
		photoBooth.Start ();
		statusbar1.Push (1,hardware.Camera.Id);
		//textview1.Visible = false;
		this.Fullscreen ();
	}

    private void PhotoBoothServiceOnPictureAdded(object sender, PictureAddedEventArgs pictureAddedEventArgs)
    {
        //Dispose any previous image in the buffer
        if (imagePhoto.Pixbuf != null)
        {
            var pixBuf = imagePhoto.Pixbuf;
            imagePhoto.Pixbuf = null;
            pixBuf.Dispose();
        }
        //Set the new image
        imagePhoto.Pixbuf = new Gdk.Pixbuf(a.PicturePath);
        ShowAll();
	}

    void OnPhotoBoothShutdownRequested(object sender, System.EventArgs e)
    {
        Stop();
    }

    private void Stop()
    {
        photoBooth.Stop();
        Application.Quit();
    }

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
        Stop();
		a.RetVal = true;
	}

	protected void OnKeyPress (object o, KeyPressEventArgs args)
	{
		this.Unfullscreen ();
	}
}
