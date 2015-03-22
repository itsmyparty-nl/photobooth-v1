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
using com.prodg.photobooth.config;
using Gtk;
using Pango;
using System.Collections.Generic;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;


public partial class MainWindow: Gtk.Window
{
	private PhotoBooth photoBooth;
	private Gdk.Cursor invisibleCursor;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();

		//Register all unhandled exception handlers
		AppDomain.CurrentDomain.UnhandledException += HandleUnhandledAppdomainException;
		GLib.ExceptionManager.UnhandledException += HandleUnhandledGlibException;
		TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

        var photoBooth = new PhotoBooth();

		PreloadImages ();
		HideCursor ();

		photoBooth.Hardware.Camera.StateChanged += OnCameraStateChanged;
        photoBooth.Hardware.Camera.BatteryWarning += OnCameraBatteryWarning;
		
		//Subscribe to the shutdown requested event 
		photoBooth.Model.ShutdownRequested += OnPhotoBoothShutdownRequested; 
		photoBooth.Service.PictureAdded += OnPhotoBoothServicePictureAdded;
        photoBooth.Hardware.PrintControl.Fired += OnPrintControlFired;
        photoBooth.Hardware.PrintTwiceControl.Fired += OnPrintTwiceControlFired;
        photoBooth.Hardware.TriggerControl.Fired += OnTriggerControlFired;
        photoBooth.Model.ErrorOccurred += OnPhotoBoothErrorOccurred;

		statusbar1.Push (1, "Waiting for camera");

		imagePhoto.Pixbuf = instructionImages ["instruction"];
		imageInstruction.Pixbuf = instructionImages ["title"];

		photoBooth.Start ();

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

	private void PreloadImages(ISettings settings)
	{
		instructionImages = new Dictionary<string, Gdk.Pixbuf> ();
		var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly ().GetName ().CodeBase).Replace("file:","");

        string eventResources = Path.Combine(path, "Resources", settings.EventId);
        string defaultResources = Path.Combine(path, "Resources");

        instructionImages.Add("title", new Gdk.Pixbuf(GetResourcePath("tilte.png", eventResources, defaultResources)));
        instructionImages.Add("indicator_1", new Gdk.Pixbuf(GetResourcePath("indicator1of4.png", eventResources, defaultResources)));
        instructionImages.Add("indicator_2", new Gdk.Pixbuf(GetResourcePath("indicator2of4.png", eventResources, defaultResources)));
        instructionImages.Add("indicator_3", new Gdk.Pixbuf(GetResourcePath("indicator3of4.png", eventResources, defaultResources)));
        instructionImages.Add("indicator_4", new Gdk.Pixbuf(GetResourcePath("indicator4of4.png", eventResources, defaultResources)));
        instructionImages.Add("instruction", new Gdk.Pixbuf(GetResourcePath("largeinstruction.png", eventResources, defaultResources)));
        instructionImages.Add("finished", new Gdk.Pixbuf(GetResourcePath("legend.png", eventResources, defaultResources)));
        instructionImages.Add("ready", new Gdk.Pixbuf(GetResourcePath("ready.png", eventResources, defaultResources)));
        instructionImages.Add("empty", new Gdk.Pixbuf(GetResourcePath("empty.png", eventResources, defaultResources)));
        instructionImages.Add("error", new Gdk.Pixbuf(GetResourcePath("error.png", eventResources, defaultResources)));
	}

    private string GetResourcePath(string fileName, string eventResources, string defaultResources)
    {
        string resourcePath = System.IO.Path.Combine(eventResources, fileName);
        if (!File.Exists(resourcePath))
        {
            System.IO.Path.Combine(defaultResources, fileName);
            if (!File.Exists(resourcePath))
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Default resource for {0} not found",
                    fileName));
            }
        }
        return resourcePath;
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

	private void Shutdown()
	{	
		photoBooth.Stop ();
		ShowCursor ();
        
        photoBooth.Dispose();

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
	    if (photoBooth != null)
	    {
	        photoBooth.Logger.LogException("Caught unhandled exception", exception);
	        if (tryToRecover)
	        {
	            try
	            {
	                imagePhoto.Pixbuf = instructionImages["error"];
                    photoBooth.Stop();
                    photoBooth.Start();
	                return true;
	            }
	            catch (Exception)
	            {
                    photoBooth.Logger.LogException("Unable to restore application, quitting", exception);
	                return false;
	            }
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
            photoBooth.Logger.LogWarning(logString);
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
