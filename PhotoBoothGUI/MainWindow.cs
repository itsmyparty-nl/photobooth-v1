using System;
using Gtk;//
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
		Console.ReadLine();

		//Initialize the photobooth
		logger = new ConsoleLogger();
		hardware = new HardwareV1(logger);
		photoBooth = new PhotoBooth(hardware, logger);

		//Start
		photoBooth.Start();
		//Wait until the photobooth is finished
		//photoBooth.Finished.WaitOne();
		//Stop
		photoBooth.Stop();
	

		this.Fullscreen ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnKeyPress (object o, KeyPressEventArgs args)
	{
		this.Unfullscreen ();
	}
}
