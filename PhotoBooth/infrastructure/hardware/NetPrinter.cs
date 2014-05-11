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
		//private PaperSize defaultPaperSize;
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
					PageSettings pageSettings = new PageSettings(pd.PrinterSettings); 
					pageSettings.PrinterResolution = new PrinterResolution(){X=300,Y=300,Kind=PrinterResolutionKind.High}; 
					pageSettings.PaperSize = pd.PrinterSettings.PaperSizes[0];
					pageSettings.Landscape = true;
					pageSettings.Margins = new Margins(0,0,0,0);
					pd.DefaultPageSettings = pageSettings;
					//defaultPaperSize = pd.PrinterSettings.PaperSizes[0];
					pd.Print();
				}
			}
			catch (Exception ex){
				logger.LogException ("Error while printing", ex);
			}
			finally {
				if (tempImageStore != null) {
					//tempImageStore.Dispose ();
					//tempImageStore = null;
				}
			}
		}

		private void PrintPage(object o, PrintPageEventArgs e)
		{
			logger.LogInfo (String.Format("Printing image ({2}x{3}) on {0}, paper size {1}, bounds ({4},{5}), dpi ({6},{7})", 
				e.PageSettings.PrinterSettings.PrinterName, e.PageSettings.PaperSize.PaperName, tempImageStore.Width,
				tempImageStore.Height, e.MarginBounds.Width, e.MarginBounds.Height, e.Graphics.DpiX, e.Graphics.DpiY));
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

			var i = tempImageStore;
			// e.MarginBounds.Width = printable width in PrinterResolution ppi
			// e.MarginBounds.Height = printable height in PrinterResolution ppi
			float graphicsWidthPx = ((e.MarginBounds.Width * e.Graphics.DpiX) / 100);
			float graphicsHeightPx = ((e.MarginBounds.Height * e.Graphics.DpiY) / 100);
			float widthFactor = i.Width /  graphicsWidthPx;  // Convert to same units (100 ppi) as e.MarginBounds.Width
			float heightFactor = i.Height / graphicsHeightPx;  // Convert to same units (100 ppi) as e.MarginBounds.Height

			logger.LogInfo (String.Format("GraphicsSize {0},{1} - Factors {2}, {3}", graphicsWidthPx, graphicsHeightPx, widthFactor, heightFactor));
			float newWidth = 0;
			float newHeight = 0;

			//if (widthFactor > 1 | heightFactor > 1) { // if the image is wider or taller than the printable area then adjust...
				if (widthFactor > heightFactor) {
					newWidth = i.Width / widthFactor;
					newHeight = i.Height / widthFactor;
				} else {
					newWidth = i.Width  / heightFactor;
					newHeight = i.Height / heightFactor;

				}
			//}
			logger.LogInfo (String.Format("e.Graphics.DrawImage (i, 0, 0, {0}, {1}", (int)newWidth, (int)newHeight));
			e.Graphics.DrawImage (i, new RectangleF(0f,0f,i.Width, i.Height), new RectangleF(0f,0f,newWidth, newHeight), GraphicsUnit.Pixel);
			e.HasMorePages = false;
			throw new Exception ();

			//e.Cancel = true;
		}
	


//		private void PrintPage_OLD(object o, PrintPageEventArgs e)
//		{
//			Point loc = new Point(0, 0);
//			e.PageSettings.Landscape = false;
//			e.PageSettings.PaperSize = defaultPaperSize;
//			e.HasMorePages = false;
//					
//			logger.LogInfo (String.Format("Printing image ({2}x{3}) on {0}, paper size {1}, bounds ({4},{5})", 
//				e.PageSettings.PrinterSettings.PrinterName, e.PageSettings.PaperSize.PaperName, tempImageStore.Width,
//				tempImageStore.Height));
//
//			var img = tempImageStore;
//			int scaleFac = 100;
//			while ((scaleFac * img.Width / img.HorizontalResolution > e.PageBounds.Width ||
//				scaleFac * img.Height / img.VerticalResolution > e.PageBounds.Height) && scaleFac > 2) {
//				scaleFac -= 1;
//			}
//			logger.LogInfo ("Calculated scale: " + scaleFac);
//			var size =  new SizeF(scaleFac * (img.Width / img.HorizontalResolution), scaleFac* (img.Height / img.VerticalResolution)); 
//
//			e.Graphics.DrawImage (tempImageStore, 0, 0, 
//				e.PageBounds.Width, e.PageBounds.Height); 
//		}

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

