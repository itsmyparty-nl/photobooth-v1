// #region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
// /*
//   PhotoBooth - an application to control a DIY photobooth
//
//   Permission is hereby granted, free of charge, to any person obtaining
//   a copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//
//   The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//   
//   Copyright 2014 Patrick Bronneberg
// */
// #endregion
//
// using com.prodg.photobooth.config;
// using Microsoft.Extensions.Logging;
// using SixLabors.ImageSharp.Processing;
// using Image = SixLabors.ImageSharp.Image;
// using RectangleF = System.Drawing.RectangleF;
//
// namespace com.prodg.photobooth.infrastructure.hardware
// {
//     /// <summary>
//     /// Printer which prints using the system.drawing.printing functionality provided by .Net
//     /// </summary>
//     public class NetPrinter : IPrinter
//     {
// 		private const int ImageDpi = 72; 
// 		private PrintAction _printAction = PrintAction.PrintToPrinter;
//         private readonly ISettings _settings;
//         private readonly ILogger<NetPrinter> _logger;
//         private Image _storedImage;
//         private Image _rotatedImage;
//         private PrintDocument _pd;
// 		private readonly ManualResetEvent _printFinished;
//
//         public NetPrinter(ISettings settings, ILogger<NetPrinter> logger)
//         {
//             _settings = settings;
//             _logger = logger;
// 			_printFinished = new ManualResetEvent (false);
//         }
//
//         /// <summary>
//         /// Print an image
//         /// </summary>
//         /// <param name="image"></param>
//         public void Print(Image image)
//         {
//             try
//             {
//                 //Store variables for printing
// 				_storedImage = image;
//                 _rotatedImage = _storedImage.Clone(context => context.RotateFlip(RotateMode.Rotate90, FlipMode.None));
// 				_printFinished.Reset();
//
// 				//Initialize the print document
// 				_pd = new PrintDocument();
//
//                 _pd.PrintPage += printDocument_PrintPage;
//                 _pd.BeginPrint += printDocument_BeginPrint;
// 				_pd.EndPrint += printDocument_EndPrint;
//                 _pd.PrinterSettings.PrinterName = _settings.PrinterName;
//                 //Set the paper settings before calling print in order to get the correct graphics object
//
//                 _pd.DefaultPageSettings.PrinterResolution = new PrinterResolution
//                 {
//                     X = 300,
//                     Y = 300
//                 };
//                 _pd.DefaultPageSettings.Color = true;
//                 _pd.PrinterSettings.PrintToFile = true;
//                 
//                 //Pick the first papersize
//                 _pd.DefaultPageSettings.PaperSize = _pd.PrinterSettings.PaperSizes[0];
//                 //pd.DefaultPageSettings.Landscape = true;
//                 _pd.DefaultPageSettings.Margins = new Margins(_settings.PrintMarginLeft, _settings.PrintMarginRight,
//                     _settings.PrintMarginTop, _settings.PrintMarginBottom);
//
// 				_pd.Print();
//
// 				_printFinished.WaitOne();
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error while printing");
// 				throw;
//             }
//         }
//
//         void printDocument_EndPrint (object sender, PrintEventArgs e)
//         {
// 			//Free all stored variables for this print
// 			_storedImage = null;
// 			_rotatedImage.Dispose();
// 			_rotatedImage = null;
//
// 			//Trigger that the print is finished
// 			_printFinished.Set ();
//         }
//
//         public void Initialize()
//         {
//             _pd = null;
//             _storedImage = null;
//             _rotatedImage = null;
//         }
//
//         public void DeInitialize()
//         {
//             _pd = null;
//             _storedImage = null;
//             _rotatedImage = null;
//         }
//
//         private void printDocument_BeginPrint(object sender, PrintEventArgs e)
//         {
//             // Save our print action so we know if we are printing 
//             // a preview or a real document.
//             _printAction = e.PrintAction;
//
//             // Set some preferences, our method should print a box with any 
//             // combination of these properties being true/false.
//             _pd.OriginAtMargins = true;   //true = soft margins, false = hard margins
//             _pd.DefaultPageSettings.Landscape = false;
//         }
//
//         private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
//         {
//             Graphics? g = e.Graphics;
//
//             // If you set printDocumet.OriginAtMargins to 'false' this event 
//             // will print the largest rectangle your printer is physically 
//             // capable of. This is often 1/8" - 1/4" from each page edge.
//             // ----------
//             // If you set printDocument.OriginAtMargins to 'false' this event
//             // will print the largest rectangle permitted by the currently 
//             // configured page margins. By default the page margins are 
//             // usually 1" from each page edge but can be configured by the end
//             // user or overridden in your code.
//             // (ex: printDocument.DefaultPageSettings.Margins)
//
//             // Grab a copy of our "soft margins" (configured printer settings)
//             // Defaults to 1 inch margins, but could be configured otherwise by 
//             // the end user. You can also specify some default page margins in 
//             // your printDocument.DefaultPageSetting properties.
//             RectangleF marginBounds = e.MarginBounds;
//
//             // Grab a copy of our "hard margins" (printer's capabilities) 
//             // This varies between printer models. Software printers like 
//             // CutePDF will have no "physical limitations" and so will return 
//             // the full page size 850,1100 for a letter page size.
//             RectangleF printableArea = e.PageSettings.PrintableArea;
//
//             // If we are print to a print preview control, the origin won't have 
//             // been automatically adjusted for the printer's physical limitations. 
//             // So let's adjust the origin for preview to reflect the printer's 
//             // hard margins.
//             if (_printAction == PrintAction.PrintToPreview)
//                 g.TranslateTransform(printableArea.X, printableArea.Y);
//
//             // Are we using soft margins or hard margins? Lets grab the correct 
//             // width/height from either the soft/hard margin rectangles. The 
//             // hard margins are usually a little wider than the soft margins.
//             // ----------
//             // Note: Margins are automatically applied to the rotated page size 
//             // when the page is set to landscape, but physical hard margins are 
//             // not (the printer is not physically rotating any mechanics inside, 
//             // the paper still travels through the printer the same way. So we 
//             // rotate in software for landscape)
//             var availableWidth =
//                 (int)
//                     Math.Floor(_pd.OriginAtMargins
//                         ? marginBounds.Width
//                         : (e.PageSettings.Landscape ? printableArea.Height : printableArea.Width));
//             var availableHeight =
//                 (int)
//                     Math.Floor(_pd.OriginAtMargins
//                         ? marginBounds.Height
//                         : (e.PageSettings.Landscape ? printableArea.Width : printableArea.Height));
//
//             _logger.LogInformation(
//                 "Printing image on {PrinterName}, printable area ({Area}), ({Width}x{Height})  bounds ({Bounds}), dpi ({DpiX},{DpiY})",
//                 e.PageSettings.PrinterSettings.PrinterName, printableArea, _rotatedImage.Width,
//                 _rotatedImage.Height, e.MarginBounds, e.Graphics!.DpiX, e.Graphics!.DpiY);
//
//             // Draw our rectangle which will either be the soft margin rectangle 
//             // or the hard margin (printer capabilities) rectangle.
//             // ----------
//             // Note: we adjust the width and height minus one as it is a zero, 
//             // zero based co-ordinates system. This will put the rectangle just 
//             // inside the available width and height.            
//
//             g.InterpolationMode = InterpolationMode.HighQualityBicubic;
//             g.SmoothingMode = SmoothingMode.HighQuality;
//             //g.DrawImage(rotatedImage, new Rectangle(0, 0, availableWidth, availableHeight));
// 			// g.DrawImage (_rotatedImage, new Rectangle (0, 0, availableWidth, availableHeight),
// 			// 	0, 0, (int)(Math.Round (_rotatedImage.Width / (ImageDpi / 100f))), (int)(Math.Round (_rotatedImage.Height / (ImageDpi / 100f))),
// 			// 	GraphicsUnit.Pixel, new ImageAttributes());
//         }
//     }
// }
//
