
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.VBox vbox1;
	
	private global::Gtk.Image imagePhoto;
	
	private global::Gtk.Image imageInstruction;
	
	private global::Gtk.Statusbar statusbar1;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.Name = "MainWindow";
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 3;
		// Container child vbox1.Gtk.Box+BoxChild
		this.imagePhoto = new global::Gtk.Image ();
		this.imagePhoto.HeightRequest = 800;
		this.imagePhoto.Name = "imagePhoto";
		this.vbox1.Add (this.imagePhoto);
		global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.imagePhoto]));
		w1.PackType = ((global::Gtk.PackType)(1));
		w1.Position = 0;
		w1.Expand = false;
		w1.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.imageInstruction = new global::Gtk.Image ();
		this.imageInstruction.WidthRequest = 1280;
		this.imageInstruction.HeightRequest = 200;
		this.imageInstruction.Name = "imageInstruction";
		this.vbox1.Add (this.imageInstruction);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.imageInstruction]));
		w2.PackType = ((global::Gtk.PackType)(1));
		w2.Position = 1;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.statusbar1 = new global::Gtk.Statusbar ();
		this.statusbar1.Name = "statusbar1";
		this.statusbar1.Spacing = 6;
		this.vbox1.Add (this.statusbar1);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.statusbar1]));
		w3.PackType = ((global::Gtk.PackType)(1));
		w3.Position = 2;
		w3.Expand = false;
		w3.Fill = false;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 1303;
		this.DefaultHeight = 1057;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
	}
}
