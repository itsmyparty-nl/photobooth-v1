using System;
using Gtk;

using com.prodg.photobooth.common;
using com.prodg.photobooth.domain;
using com.prodg.photobooth.infrastructure.hardware;

public partial class MainWindow: Gtk.Window
{
	private ILogger logger;
	private IHardware hardware;
	private PhotoBooth photoBooth;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		//Keep the application open
		Console.ReadLine ();

		//Initialize the photobooth
		logger = new TextBoxLogger (textview1.Buffer);
		hardware = new GuiHardware (logger, buttonTrigger, buttonPrint, buttonExit);
		photoBooth = new PhotoBooth (hardware, logger);

		photoBooth.SessionChanged += OnPhotoboothSessionChangedEvent;

		//Start
		photoBooth.Start ();
		statusbar1.Push (1,hardware.Camera.Id);
		textview1.Visible = false;
		this.Fullscreen ();
	}
				
	protected void OnPhotoboothSessionChangedEvent (object sender, SessionChangeEventArgs a)
	{
		a.Session.PictureAdded += OnSessionPictureAdded;
		a.Session.Finished += OnSessionPictureAdded;
	}

	protected void OnSessionPictureAdded (object sender, PictureAddedEventArgs a)
	{
		//Dispose any previous image in the buffer
		if (imagePhoto.Pixbuf != null) {
			var pixBuf = imagePhoto.Pixbuf;
			imagePhoto.Pixbuf =	null;
			pixBuf.Dispose ();
		}
		//Set the new image
		imagePhoto.Pixbuf = new Gdk.Pixbuf (a.PicturePath);
		ShowAll ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		photoBooth.Stop ();

		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnKeyPress (object o, KeyPressEventArgs args)
	{
		this.Unfullscreen ();
	}
}
