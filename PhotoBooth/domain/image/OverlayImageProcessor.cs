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
// using Microsoft.Extensions.Logging;
// using SixLabors.ImageSharp;
//
// namespace com.prodg.photobooth.domain.image
// {
//     /// <summary>
//     /// An image processor processes an image by applying a filter
//     /// </summary>
//     public class OverlayImageProcessor : ISingleImageProcessor, IDisposable
//     {
//         private readonly ILogger<OverlayImageProcessor> _logger;
//         private readonly string _overlayImageFileName;
//         private Image _overlayImage;
//
//         /// <summary>
//         /// C'tor
//         /// </summary>
//         /// <param name="logger"></param>
//         /// <param name="overlayImageFileName"></param>
//         public OverlayImageProcessor(ILogger<OverlayImageProcessor> logger, string overlayImageFileName)
//         {
//             _logger = logger;
//             _overlayImageFileName = overlayImageFileName;
//
//             logger.LogDebug("Creating OverlayImageProcessor with {OverlayImageFileName}", overlayImageFileName);
//             if (string.IsNullOrWhiteSpace(overlayImageFileName) || !File.Exists(overlayImageFileName))
//             {
//                 throw new FileNotFoundException("overlay image does not exist: " + overlayImageFileName);
//             }
//
//             _overlayImage = Image.Load(overlayImageFileName);
//         }
//
//         /// <summary>
//         /// Processes the images into a single image
//         /// </summary>
//         public Image Process(PhotoSession? session, Image image)
//         {            
//             _logger.LogInformation("Applying overlay on image: {OverlayImage}" , Path.GetFileNameWithoutExtension(_overlayImageFileName));
//
//             if (image == null)
//             {
//                 throw new ArgumentNullException(nameof(image), "image may not be null");
//             }
//
//             //get a graphics object from the image so we can draw on it
//             using (var graphics = Graphics.FromImage(image))
//             {
//                 var srcRectangle = new Rectangle(0, 0, image.Width, image.Height);
//                 int xStart = (int) Math.Round((image.Width/2f) - (_overlayImage.Width/2f));
//                 int yStart = (int) Math.Round((image.Height/2f) - (_overlayImage.Height/2f));
//                 //int xStart = (int) Math.Round(image.Width/2f);
//                 //int yStart = (int) Math.Round(image.Height/2f);
//                 
//                 //graphics.DrawImage(overlayImage, srcRectangle, xStart, yStart, overlayImage.Width, overlayImage.Height,
//                 //    GraphicsUnit.Pixel, attributes);
//                             graphics.SmoothingMode = SmoothingMode.HighQuality;
//             graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
//                 graphics.DrawImage(_overlayImage, xStart, yStart,
//                     new Rectangle(0, 0, _overlayImage.Width, _overlayImage.Height), GraphicsUnit.Pixel);
//             }
//
//             return image;
//         }
//
//         #region IDisposable Implementation
//
// 		bool disposed;
//
// 		public void Dispose ()
// 		{
// 			Dispose (true);
// 			GC.SuppressFinalize (this);
// 		}
//
// 		private void Dispose (bool disposing)
// 		{
// 		    if (!disposed)
// 		    {
// 		        if (disposing)
// 		        {
// 		            // Clean up managed objects
// 		            if (_attributes != null)
// 		            {
// 		               _attributes.Dispose();
//                        _attributes = null;
// 		            }
//
//                     if (_overlayImage != null)
//                     {
//                         _overlayImage.Dispose();
//                         _overlayImage = null;
//                     }
// 		        }
// 		        // clean up any unmanaged objects
// 		        disposed = true;
// 		    }
// 		}
//
//         ~OverlayImageProcessor()
// 		{
// 			Dispose (false);
// 		}
// 		
// 		#endregion
//
//     }
// }
