using System;
using Gtk;

namespace PhotoBoothGUI
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.ModifyBg (StateType.Normal, new Gdk.Color (0,0, 0));
			win.Show ();
			Application.Run ();
		}
	}
}
