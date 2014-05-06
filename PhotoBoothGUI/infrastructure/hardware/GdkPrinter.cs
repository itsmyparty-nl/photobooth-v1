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
using com.prodg.photobooth.common;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class GdkPrinter: IPrinter
	{
		private readonly ISettings settings;
		private readonly ILogger logger;
		private Printer printer; 
		public GdkPrinter (ISettings settings, ILogger logger)
		{
			this.settings = settings;
			this.logger = logger;

			Printer.EnumeratePrinters (new PrinterFunc (EnumeratePrinterFunction),true);

			//Gtk.PaperSize[] PapersSizes =  Gtk.PaperSize.GetPaperSizes(false);
			//foreach(Gtk.PaperSize pz in PapersSizes){
			//		logger.LogInfo(pz.Name);
			//}

			//if (printer == null) {
			//	throw new Exception ("Printer not found");
			//}
		}

		//private Gtk.PrintOperationResult Print(Gtk.PrintOperationAction action, string FileName){
		//	Gtk.PaperSize PaperSize= new Gtk.PaperSize("na_letter");
		//		Gtk.PageSetup PageSetup = new Gtk.PageSetup();
		//		PageSetup.PaperSize=PaperSize;
		//	Gtk.PrintOperation PrintOperation = new Gtk.PrintOperation();
		//	PrintOperation.DefaultPageSetup=PageSetup;
		//	PrintOperation.Unit = Gtk.Unit.Mm;
			//PrintOperation.BeginPrint+= PrintOperationHandleBeginPrint;
			//PrintOperation.DrawPage+= PrintOperationHandleDrawPage;
		//	if(action == Gtk.PrintOperationAction.Export){
		//		PrintOperation.ExportFilename=FileName;
		//	}
			//return PrintOperation.Run(action, win);
	//}

		/// <summary>
		/// Print an image
		/// </summary>
		/// <param name="image"></param>
		public void Print(System.Drawing.Image image)
		{
			var imageBit = default(byte[]);

			using (System.IO.MemoryStream stream = new System.IO.MemoryStream ()) { 
				image.Save (stream,  System.Drawing.Imaging.ImageFormat.Bmp);
				imageBit = stream.ToArray ();
			}


			var print = new PrintOperation ();
			print.BeginPrint += (obj, a) => {
				Gtk.PaperSize paperSize= new PaperSize("om_small-photo");
				Gtk.PageSetup pageSetup = new Gtk.PageSetup();
				pageSetup.PaperSize=paperSize;


				print.NPages = 1;
				print.PrintSettings.PaperSize = paperSize;
				//print.PrintSettings.PaperSize = new PaperSize("na_10x15");
				print.PrintSettings.Printer = printer.Name;
				print.Unit = Gtk.Unit.Mm;
				print.DefaultPageSetup = pageSetup;

			};

			print.DrawPage += (obj, a) => {
				using (PrintContext context = a.Context) {
					using (var pixBuf = new Gdk.Pixbuf (imageBit, image.Width, image.Height)) {
						Cairo.Context cr = context.CairoContext;

						cr.MoveTo (0, 0);
						Gdk.CairoHelper.SetSourcePixbuf (cr, pixBuf, image.Width, image.Height);
						cr.Paint ();

						((IDisposable)cr).Dispose ();
					}
				}
			};
			print.EndPrint += (obj, a) => {
			};

			print.Run (PrintOperationAction.Print, null);
		}

		public void Initialize (){
			//Do nothing
		}

		public void DeInitialize(){
			//Do nothing
		}

		public bool EnumeratePrinterFunction(Printer foundPrinter)
		{
			if (foundPrinter.Name.Contains ("780")) {
				logger.LogInfo ("Selected printer:" + foundPrinter.Name);
				printer = foundPrinter;
				logger.LogInfo ("Printer active: " + foundPrinter.IsActive); 
				foreach (PageSetup pageSetup in foundPrinter.ListPapers ()) {
					PaperSize size = pageSetup.PaperSize;
					logger.LogInfo ("Paper size: " + size.DisplayName);
				}
				return true;

			} else {
				logger.LogInfo ("Ignored printer:" + foundPrinter.Name);
				return false;
			}
		}
	}
}

