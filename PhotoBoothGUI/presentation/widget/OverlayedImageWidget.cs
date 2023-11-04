using System;
using Gtk;
using Pango;

namespace com.prodg.photobooth.presentation.widget
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OverlayedImageWidget : Gtk.Image
	{
		private Label label;

		public string Text {
			get { return label.Text; }
			set { label.Text = value; }
		}

		public FontDescription Font {
			set { label.ModifyFont(value); }
		}

		public Gdk.Color FgColor {
			set	{ label.ModifyFg (StateType.Normal, value); }
		}

		public OverlayedImageWidget ()
		{
			// Set the initial text to finish preparing the Pango.Layout.
			label = new Gtk.Label ();
			SetDefaultStyle ();
			Text = String.Empty;
		}

		private void SetDefaultStyle()
		{
			Pango.FontDescription fontdesc = new Pango.FontDescription();
			fontdesc.Family = "Sans";
			fontdesc.Size = (int)(32*Pango.Scale.PangoScale);
			fontdesc.Weight = Pango.Weight.Semibold;
			Font = fontdesc;

			Gdk.Color fontcolor = new Gdk.Color(255,255,255);
			FgColor = fontcolor;
		}

		// protected override bool OnExposeEvent (Gdk.EventExpose Event)
		// {
		// 	//Do we need a check on this.IsRealized
		// 	bool baseExposeResult = base.OnExposeEvent (Event);
		// 	if (baseExposeResult && !string.IsNullOrEmpty (Text)) {
		// 	
		// 		// Determine the target area.
		// 		Gdk.Rectangle targetRectangle = new Gdk.Rectangle (0, 0, Allocation.Width, Allocation.Height);
		//
		// 		//Draw the label without shadows etc on top of the image
		// 		Gtk.Style.PaintLayout (label.Style,  Event.Window, Gtk.StateType.Insensitive, false, targetRectangle, label, null, Style.XThickness, Style.YThickness, label.Layout);
		// 		// Send back that this expose event has been completely handled.
		// 		return true;
		// 	} else {
		// 		return baseExposeResult;
		// 	}
		// }
	}

}

