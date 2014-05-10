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
using com.prodg.photobooth.config;
using com.prodg.photobooth.common;
using System.Drawing.Printing;
using System.Drawing;

namespace com.prodg.photobooth.infrastructure.hardware
{
	public class NetPrinter: IPrinter
	{
		private readonly ISettings settings;
		private readonly ILogger logger;
	
		public NetPrinter (ISettings settings, ILogger logger)
		{
			this.settings = settings;
			this.logger = logger;
		}

		private Image tempImageStore; 
		private PaperSize defaultPaperSize;
		/// <summary>
		/// Print an image
		/// </summary>
		/// <param name="image"></param>
		public void Print(System.Drawing.Image image)
		{
			try
			{
				tempImageStore = new Bitmap(image);

				using (PrintDocument pd = new PrintDocument ()) {
					pd.PrintPage += PrintPage;
					pd.PrinterSettings.PrinterName = settings.PrinterName;
					pd.PrinterSettings.DefaultPageSettings.PaperSize = pd.PrinterSettings.PaperSizes[0];
					defaultPaperSize = pd.PrinterSettings.PaperSizes[0];
					pd.Print();
				}
			}
			catch (Exception ex){
				logger.LogException ("Error while printing", ex);
			}
			finally {
				if (tempImageStore != null) {
					//tempImageStore.Dispose ();
					tempImageStore = null;
				}
			}
		}

		private void PrintPage(object o, PrintPageEventArgs e)
		{
			Point loc = new Point(0, 0);
			e.PageSettings.Landscape = true;
			e.PageSettings.PaperSize = defaultPaperSize;
			e.HasMorePages = false;
		
			logger.LogInfo (String.Format("Printing image ({2}x{3}) on {0}, paper size {1} " + 
				e.PageSettings.PrinterSettings.PrinterName, e.PageSettings.PaperSize.PaperName, tempImageStore.Width,
				tempImageStore.Height));
			
			e.Graphics.DrawImage(tempImageStore, loc);     
		}

		public void Initialize (){
			//Do nothing
		}

		public void DeInitialize(){
			//Do nothing
		}

		private void LogAvailablePaperSizes()
		{
			using (PrintDocument pd = new PrintDocument ()) {
				pd.PrintPage += PrintPage;
				pd.PrinterSettings.PrinterName = settings.PrinterName;
				//PaperSize pkSize;
				for (int i = 0; i < pd.PrinterSettings.PaperSizes.Count; i++) {
					var pkSize = pd.PrinterSettings.PaperSizes [i];
					logger.LogInfo ("Paper name: " + pkSize.PaperName);
					logger.LogInfo ("Paper kind: " + pkSize.Kind.ToString ());
					logger.LogInfo ("Paper Width (inch): " + pkSize.Width);
					logger.LogInfo ("Paper Height (inch): " + pkSize.Height);
				}

			}
		}
	}
}

