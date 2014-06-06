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
using System.Drawing;

namespace com.prodg.photobooth.infrastructure.hardware
{
    /// <summary>
    /// Printer which prints using the system.drawing.printing functionality provided by .Net
    /// </summary>
    public class NetPrinter : IPrinter
    {
        private PrintAction printAction = PrintAction.PrintToPrinter;
        private readonly ISettings settings;
        private readonly ILogger logger;
        private Image storedImage;
        private PrintDocument pd;

        public NetPrinter(ISettings settings, ILogger logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        /// <summary>
        /// Print an image
        /// </summary>
        /// <param name="image"></param>
        public void Print(Image image)
        {
            try
            {
                storedImage = image;
                pd = new PrintDocument();

                pd.PrintPage += printDocument_PrintPage2;
                pd.BeginPrint += printDocument_BeginPrint2;
                pd.PrinterSettings.PrinterName = settings.PrinterName;
                //Set the paper settings before calling print in order to get the correct graphics object

                // We ALWAYS want true here, as we will implement the 
                // margin limitations later in code.
                //pd.OriginAtMargins = true;

                pd.DefaultPageSettings.PrinterResolution = new PrinterResolution
                {
                    X = 300,
                    Y = 300,
                    Kind = PrinterResolutionKind.High
                };
                //Pick the first papersize
                pd.DefaultPageSettings.PaperSize = pd.PrinterSettings.PaperSizes[0];
                //pd.DefaultPageSettings.Landscape = true;
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                pd.Print();

            }
            catch (Exception ex)
            {
                logger.LogException("Error while printing", ex);
            }
            finally
            {
                storedImage = null;
            }
        }

        private void PrintPage(object o, PrintPageEventArgs e)
        {
            
            //Set print area size and margins
            var printHeight = e.PageSettings.PaperSize.Height - e.PageSettings.Margins.Top - e.PageSettings.Margins.Bottom;
            var printWidth = e.PageSettings.PaperSize.Width - e.PageSettings.Margins.Left - e.PageSettings.Margins.Right;
            var leftMargin = e.PageSettings.Margins.Left;  //X
            var rightMargin = e.PageSettings.Margins.Top;  //Y
            
            //Check if the user selected to print in Landscape mode
            //if they did then we need to swap height/width parameters
            if (e.PageSettings.Landscape)
            {
                var tmp = printHeight;
                printHeight = printWidth;
                printWidth = tmp;
            }

            // e.MarginBounds.Width = printable width in PrinterResolution ppi
            // e.MarginBounds.Height = printable height in PrinterResolution ppi
            logger.LogInfo(
                String.Format("Printing image ({2}x{3}) on {0}, paper size {1}, bounds ({4},{5}), dpi ({6},{7})",
                    e.PageSettings.PrinterSettings.PrinterName, e.PageSettings.PaperSize.PaperName, storedImage.Width,
                    storedImage.Height, e.MarginBounds.Width, e.MarginBounds.Height, e.Graphics.DpiX, e.Graphics.DpiY));

            //Scale the image and draw it on the print event's graphics object
            //ScaleAndCenterToPage(e.Graphics, e.MarginBounds, storedImage);

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(storedImage, new Rectangle());

            //Tell the print method that this the last and only page 
            e.HasMorePages = false;
        }

        /// <summary>
        /// Scale and center the image to occupy most of the graphics object while maintaining the aspect ratio
        /// </summary>
        /// <remarks>This code is based on the assumption that the dpi properties are set correctly on the graphics object</remarks>
        protected void ScaleAndCenterToPage(Graphics graphics, Rectangle bounds, Image image)
        {
            ////Calculate the width in pixels from the bounds width.
            ////According to MSDN, the bounds are defined in 100ths of an inch
            //var graphicsWidthPx = 1800f; //((bounds.Width*graphics.DpiX)/100);
            //var graphicsHeightPx = 1200f; //((bounds.Height*graphics.DpiY)/100);

            ////Calculate the scaling factor in both dimensions
            //var widthFactor = image.Width/graphicsWidthPx;
            //var heightFactor = image.Height/graphicsHeightPx;

            ////Determine in which dimension the image fits after scaling
            //RectangleF destRectangle;
            //if (widthFactor > heightFactor)
            //{
            //    //Wide images
            //    float startY = (graphicsHeightPx - (image.Height/widthFactor))/2;
            //    destRectangle = new RectangleF(0, startY, image.Width/widthFactor, image.Height/widthFactor);
            //}
            //else
            //{
            //    //Tall images
            //    float startX = (graphicsWidthPx - (image.Width/heightFactor))/2;
            //    destRectangle = new RectangleF(startX, 0, image.Width/heightFactor, image.Height/heightFactor);
            //}

          

            ////Scale with high quality interpolation to achieve the best print
            //graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            //graphics.DrawImage(image, destRectangle);
        }

        public void Initialize()
        {
            //Do nothing
        }

        public void DeInitialize()
        {
            //Do nothing
        }

        private void LogAvailablePaperSizes()
        {
            using (var pd = new PrintDocument())
            {
                pd.PrintPage += PrintPage;
                pd.PrinterSettings.PrinterName = settings.PrinterName;
                //PaperSize pkSize;
                for (int i = 0; i < pd.PrinterSettings.PaperSizes.Count; i++)
                {
                    var pkSize = pd.PrinterSettings.PaperSizes[i];
                    logger.LogInfo("Paper name: " + pkSize.PaperName);
                    logger.LogInfo("Paper kind: " + pkSize.Kind.ToString());
                    logger.LogInfo("Paper Width (inch): " + pkSize.Width);
                    logger.LogInfo("Paper Height (inch): " + pkSize.Height);
                }

            }
        }


        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
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

            // Grab a copy of our "hard margins" (printer's capabilities) 
            // This varies between printer models. Software printers like 
            // CutePDF will have no "physical limitations" and so will return 
            // the full page size 850,1100 for a letter page size.
            RectangleF printableArea = e.PageSettings.PrintableArea;
            RectangleF realPrintableArea = new RectangleF(
                (e.PageSettings.Landscape ? printableArea.Y : printableArea.X),
                (e.PageSettings.Landscape ? printableArea.X : printableArea.Y),
                (e.PageSettings.Landscape ? printableArea.Height : printableArea.Width),
                (e.PageSettings.Landscape ? printableArea.Width : printableArea.Height)
                );

            // If we are printing to a print preview control, the origin won't have 
            // been automatically adjusted for the printer's physical limitations. 
            // So let's adjust the origin for preview to reflect the printer's 
            // hard margins.
            // ----------
            // Otherwise if we really are printing, just use the soft margins.
            g.TranslateTransform(
                ((printAction == PrintAction.PrintToPreview) ? realPrintableArea.X : 0) - e.MarginBounds.X,
                ((printAction == PrintAction.PrintToPreview) ? realPrintableArea.Y : 0) - e.MarginBounds.Y
            );

            // Draw the printable area rectangle in PURPLE
            Rectangle printedPrintableArea = Rectangle.Truncate(realPrintableArea);
            printedPrintableArea.Width--;
            printedPrintableArea.Height--;
            g.DrawRectangle(Pens.Purple, printedPrintableArea);

            // Grab a copy of our "soft margins" (configured printer settings)
            // Defaults to 1 inch margins, but could be configured otherwise by 
            // the end user. You can also specify some default page margins in 
            // your printDocument.DefaultPageSetting properties.
            RectangleF marginBounds = e.MarginBounds;

            // This intersects the desired margins with the printable area rectangle. 
            // If the margins go outside the printable area on any edge, it will be 
            // brought in to the appropriate printable area.
            marginBounds.Intersect(realPrintableArea);

            // Draw the margin rectangle in RED
            Rectangle printedMarginArea = Rectangle.Truncate(marginBounds);
            printedMarginArea.Width--;
            printedMarginArea.Height--;
            g.DrawRectangle(Pens.Red, printedMarginArea);

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(storedImage, printedPrintableArea);
        }

        private void printDocument_BeginPrint2(object sender, PrintEventArgs e)
        {
            // Save our print action so we know if we are printing 
            // a preview or a real document.
            printAction = e.PrintAction;

            // Set some preferences, our method should print a box with any 
            // combination of these properties being true/false.
            pd.OriginAtMargins = true;   //true = soft margins, false = hard margins
            pd.DefaultPageSettings.Landscape = true;
        }

        private void printDocument_PrintPage2(object sender, PrintPageEventArgs e)
        {
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
            int availableWidth =
                (int)
                    Math.Floor(pd.OriginAtMargins
                        ? marginBounds.Width
                        : (e.PageSettings.Landscape ? printableArea.Height : printableArea.Width));
            int availableHeight =
                (int)
                    Math.Floor(pd.OriginAtMargins
                        ? marginBounds.Height
                        : (e.PageSettings.Landscape ? printableArea.Width : printableArea.Height));

            logger.LogInfo(
                String.Format("Printing image ({2}x{3}) on {0}, printable area ({1}), bounds ({4}), dpi ({5},{6})",
                    e.PageSettings.PrinterSettings.PrinterName, printableArea, storedImage.Width,
                    storedImage.Height, e.MarginBounds, e.Graphics.DpiX, e.Graphics.DpiY));


            throw new Exception("dummy");
            // Draw our rectangle which will either be the soft margin rectangle 
            // or the hard margin (printer capabilities) rectangle.
            // ----------
            // Note: we adjust the width and height minus one as it is a zero, 
            // zero based co-ordinates system. This will put the rectangle just 
            // inside the available width and height.
            //g.DrawRectangle(Pens.Red, 0, 0, availableWidth - 1, availableHeight - 1);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(storedImage, new Rectangle(0, 0, availableWidth - 1, availableHeight - 1));
        }
    }
}

