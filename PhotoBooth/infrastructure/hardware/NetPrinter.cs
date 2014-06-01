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
        private readonly ISettings settings;
        private readonly ILogger logger;
        private Image storedImage;

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
                using (var pd = new PrintDocument())
                {
                    pd.PrintPage += PrintPage;
                    pd.PrinterSettings.PrinterName = settings.PrinterName;
                    //Set the paper settings before calling print in order to get the correct graphics object
                    var pageSettings = new PageSettings(pd.PrinterSettings)
                    {
                        PrinterResolution = new PrinterResolution {X = 300, Y = 300, Kind = PrinterResolutionKind.High},
                        //Pick the first papersize
                        PaperSize = pd.PrinterSettings.PaperSizes[0],
                        Landscape = true,
                        Margins = new Margins(0, 0, 0, 0)
                    };
                    pd.DefaultPageSettings = pageSettings;
                    pd.Print();
                }
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

            // e.MarginBounds.Width = printable width in PrinterResolution ppi
            // e.MarginBounds.Height = printable height in PrinterResolution ppi
            logger.LogInfo(
                String.Format("Printing image ({2}x{3}) on {0}, paper size {1}, bounds ({4},{5}), dpi ({6},{7})",
                    e.PageSettings.PrinterSettings.PrinterName, e.PageSettings.PaperSize.PaperName, storedImage.Width,
                    storedImage.Height, e.MarginBounds.Width, e.MarginBounds.Height, e.Graphics.DpiX, e.Graphics.DpiY));

            //Scale the image and draw it on the print event's graphics object
            ScaleAndCenterToPage(e.Graphics, e.MarginBounds, storedImage);

            //Tell the print method that this the last and only page 
            e.HasMorePages = false;
        }

        /// <summary>
        /// Scale and center the image to occupy most of the graphics object while maintaining the aspect ratio
        /// </summary>
        /// <remarks>This code is based on the assumption that the dpi properties are set correctly on the graphics object</remarks>
        protected void ScaleAndCenterToPage(Graphics graphics, Rectangle bounds, Image image)
        {
            //Calculate the width in pixels from the bounds width.
            //According to MSDN, the bounds are defined in 100ths of an inch
            var graphicsWidthPx = ((bounds.Width*graphics.DpiX)/100);
            var graphicsHeightPx = ((bounds.Height*graphics.DpiY)/100);


            //Calculate the scaling factor in both dimensions
            var widthFactor = image.Width/graphicsWidthPx;
            var heightFactor = image.Height/graphicsHeightPx;


            //Determine in which dimension the image fits after scaling
            RectangleF destRectangle;
            if (widthFactor > heightFactor)
            {
                //Wide images
                float startY = (graphicsHeightPx - (image.Height/widthFactor))/2;
                destRectangle = new RectangleF(0, startY, image.Width/widthFactor, image.Height/widthFactor);
            }
            else
            {
                //Tall images
                float startX = (graphicsWidthPx - (image.Width/heightFactor))/2;
                destRectangle = new RectangleF(startX, 0, image.Width/heightFactor, image.Height/heightFactor);
            }

            //Scale with high quality interpolation to achieve the best print
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.DrawImage(image, destRectangle, new RectangleF(0f, 0f, image.Width, image.Height),
                GraphicsUnit.Pixel);
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
    }
}

