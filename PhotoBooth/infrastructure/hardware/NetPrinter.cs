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
using System.Drawing.Drawing2D;
using com.prodg.photobooth.config;
using com.prodg.photobooth.common;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;

namespace com.prodg.photobooth.infrastructure.hardware
{
    /// <summary>
    /// Printer which prints using the system.drawing.printing functionality provided by .Net
    /// </summary>
    public class NetPrinter : IPrinter
    {
        private const int ImageDpi = 72;
        private const int PrinterDpi = 300;
        private PrintAction printAction = PrintAction.PrintToPrinter;
        private readonly ISettings settings;
        private readonly ILogger logger;
        private Image storedImage;
        private Image rotatedImage;
        private PrintDocument pd;
        private ManualResetEvent printFinished;
        private ImageAttributes attributes;

        public NetPrinter(ISettings settings, ILogger logger)
        {
            this.settings = settings;
            this.logger = logger;
            printFinished = new ManualResetEvent(false);
            attributes = new ImageAttributes();
        }

        /// <summary>
        /// Print an image
        /// </summary>
        /// <param name="image"></param>
        public void Print(Image image)
        {
            try
            {
                //Store variables for printing
                storedImage = image;
                rotatedImage = (Image) storedImage.Clone();
                rotatedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                printFinished.Reset();

                //Initialize the print document
                pd = new PrintDocument();

                pd.PrintPage += printDocument_PrintPage;
                pd.BeginPrint += printDocument_BeginPrint;
                pd.EndPrint += printDocument_EndPrint;
                pd.PrinterSettings.PrinterName = settings.PrinterName;
                pd.DocumentName = "PhotoPrint";
                //Set the paper settings before calling print in order to get the correct graphics object

                logger.LogInfo(pd.DefaultPageSettings.ToString());
                logger.LogInfo(pd.PrinterSettings.ToString());

                pd.DefaultPageSettings.PrinterResolution = new PrinterResolution
                {
                    X = PrinterDpi,
                    Y = PrinterDpi,
                    Kind = PrinterResolutionKind.High
                };
                //Pick the first papersize
                pd.DefaultPageSettings.PaperSize = pd.PrinterSettings.PaperSizes[0];
                //pd.DefaultPageSettings.PaperSize = new PaperSize("Photo", 394, 583);

                int maxHeight = pd.DefaultPageSettings.PaperSize.Height * PrinterDpi / 100;
                int maxWidth = pd.DefaultPageSettings.PaperSize.Width * PrinterDpi / 100;
                var img = ResizeImage(rotatedImage, maxWidth, maxHeight);
                rotatedImage.Dispose();
                rotatedImage = img;
                
                //pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.Margins = new Margins(settings.PrintMarginLeft, settings.PrintMarginRight,
                    settings.PrintMarginTop, settings.PrintMarginBottom);

                pd.DefaultPageSettings.Color = true;

                logger.LogInfo(pd.DefaultPageSettings.ToString());
                logger.LogInfo(pd.PrinterSettings.ToString());

                pd.Print();

                printFinished.WaitOne();
            }
            catch (Exception ex)
            {
                logger.LogException("Error while printing", ex);
                throw;
            }
        }

        void printDocument_EndPrint(object sender, PrintEventArgs e)
        {
            logger.LogInfo("printDocument_EndPrint");
            //Free all stored variables for this print
            storedImage = null;
            rotatedImage.Dispose();
            rotatedImage = null;

            //Trigger that the print is finished
            printFinished.Set();
        }

        public void Initialize()
        {
            pd = null;
            storedImage = null;
            rotatedImage = null;
        }

        public void DeInitialize()
        {
            pd = null;
            storedImage = null;
            rotatedImage = null;
        }

        private void printDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            logger.LogInfo("printDocument_BeginPrint");
            // Save our print action so we know if we are printing 
            // a preview or a real document.
            printAction = e.PrintAction;

            // Set some preferences, our method should print a box with any 
            // combination of these properties being true/false.
            pd.OriginAtMargins = true; //true = soft margins, false = hard margins
            pd.DefaultPageSettings.Landscape = false;
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            logger.LogInfo("printDocument_PrintPage");
            logger.LogInfo(e.ToString());
            logger.LogInfo(pd.ToString());
            Graphics g = e.Graphics;

            // If you set printDocumet.OriginAtMargins to 'false' this event 
            // will print the largest rectangle your printer is physically 
            // capable of. This is often 1/8" - 1/4" from each page edge.
            // ----------
            // If you set printDocument.OriginAtMargins to 'false' this event
            // will print the largest rectangle permitted by the currently 
            // configured page margins. By default the page margins are 
            // usually 1" from each page edge but can be configured by the end
            // user or overridden in your code.
            // (ex: printDocument.DefaultPageSettings.Margins)

            // Grab a copy of our "soft margins" (configured printer settings)
            // Defaults to 1 inch margins, but could be configured otherwise by 
            // the end user. You can also specify some default page margins in 
            // your printDocument.DefaultPageSetting properties.
            RectangleF marginBounds = e.MarginBounds;

            // Grab a copy of our "hard margins" (printer's capabilities) 
            // This varies between printer models. Software printers like 
            // CutePDF will have no "physical limitations" and so will return 
            // the full page size 850,1100 for a letter page size.
            RectangleF printableArea = e.PageSettings.PrintableArea;

            // If we are print to a print preview control, the origin won't have 
            // been automatically adjusted for the printer's physical limitations. 
            // So let's adjust the origin for preview to reflect the printer's 
            // hard margins.
            if (printAction == PrintAction.PrintToPreview)
                g.TranslateTransform(printableArea.X, printableArea.Y);

            // Are we using soft margins or hard margins? Lets grab the correct 
            // width/height from either the soft/hard margin rectangles. The 
            // hard margins are usually a little wider than the soft margins.
            // ----------
            // Note: Margins are automatically applied to the rotated page size 
            // when the page is set to landscape, but physical hard margins are 
            // not (the printer is not physically rotating any mechanics inside, 
            // the paper still travels through the printer the same way. So we 
            // rotate in software for landscape)
            var availableWidth =
                (int)
                Math.Floor(pd.OriginAtMargins
                    ? marginBounds.Width
                    : (e.PageSettings.Landscape ? printableArea.Height : printableArea.Width));
            var availableHeight =
                (int)
                Math.Floor(pd.OriginAtMargins
                    ? marginBounds.Height
                    : (e.PageSettings.Landscape ? printableArea.Width : printableArea.Height));

            logger.LogInfo(
                String.Format(
                    "Printing image ({2}x{3}) on {0}, printable area ({1}), bounds ({4}), dpi ({5},{6}), g.size ({7}, {8})",
                    e.PageSettings.PrinterSettings.PrinterName, printableArea, rotatedImage.Width,
                    rotatedImage.Height, e.MarginBounds, e.Graphics.DpiX, e.Graphics.DpiY, availableWidth,
                    availableHeight));

            // Draw our rectangle which will either be the soft margin rectangle 
            // or the hard margin (printer capabilities) rectangle.
            // ----------
            // Note: we adjust the width and height minus one as it is a zero, 
            // zero based co-ordinates system. This will put the rectangle just 
            // inside the available width and height.            

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            //g.DrawImage(rotatedImage, new Rectangle(0, 0, availableWidth, availableHeight));
            g.DrawImage(rotatedImage, new Rectangle(0, 0, availableWidth, availableHeight),
                0, 0, (int) (Math.Round(rotatedImage.Width / (ImageDpi / 100f))),
                (int) (Math.Round(rotatedImage.Height / (ImageDpi / 100f))),
                GraphicsUnit.Pixel, attributes);

            logger.LogInfo("printDocument_PrintPage finished");
        }


        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            // Keep aspect ratio
            float scale = Math.Min(1.0f, Math.Min((float) height / image.Height, (float) width / image.Width));
            width = (int)Math.Round(image.Width * scale);
            height = (int)Math.Round(image.Height * scale);

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(ImageDpi, ImageDpi);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}

