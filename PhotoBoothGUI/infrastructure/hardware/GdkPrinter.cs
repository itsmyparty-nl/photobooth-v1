﻿#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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
	public class GtkPrinter: IPrinter
	{
		private readonly ISettings settings;
		private readonly ILogger logger;

		public GtkPrinter (ISettings settings, ILogger logger)
		{
			this.settings = settings;
			this.logger = logger;
		}

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
				print.NPages = 1;
				//print.DefaultPageSetup.PaperSize = PaperSize.
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
	}
}

