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

using com.prodg.photobooth.config;
using Microsoft.Extensions.Logging;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace com.prodg.photobooth.domain.image
{
    /// <summary>
    /// An image processor processes an image by applying a filter
    /// </summary>
    public class QrCodeOverlayImageProcessor : ISingleImageProcessor
    {
        private readonly ILogger<QrCodeOverlayImageProcessor> _logger;
        private readonly string _baseUrl;
        private readonly string _eventId;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public QrCodeOverlayImageProcessor(ILogger<QrCodeOverlayImageProcessor> logger, ISettings settings)
        {
            _logger = logger;
                   
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append(settings.OffloadAddress.TrimEnd('/'));
            _baseUrl = urlBuilder.ToString();
            _eventId = settings.ApiEventId;
        
            _logger.LogInformation("QR Code BaseURL configured to URL '{BaseUrl}'", _baseUrl);
        }

        public Image Process(Image image, int sessionIndex)
        {
	        if (image == null)
	        {
		        throw new ArgumentNullException(nameof(image), "image may not be null");
	        }
	        
	        var overlayImage = PrepareQrCode(sessionIndex);
	        if (overlayImage == null)
	        {
		        return image;
	        }
	        //get a graphics object from the image so we can draw on it
            
	        image.Mutate(x =>
	        {
		        int xStart = (int) Math.Round((image.Width/2f) - (overlayImage.Width/2f));
		        int yStart = (int) Math.Round((image.Height/2f) - (overlayImage.Height/2f));
		        x.DrawImage(overlayImage, new Point(xStart, yStart), 1);
	        });

	        return image;
        }
        
        /// <summary>
        /// Processes the images into a single image
        /// </summary>
        public Image Process(IPhotoSession? session, Image image)
        {
	        if (session == null)
	        {
		        throw new ArgumentNullException(nameof(session), "session may not be null");
	        }

	        return Process(image, session.Id);
        }
        
        private Image? PrepareQrCode(int sessionIndex)
        {
	        var url = $"{_baseUrl}/events/{_eventId}/sessions/{sessionIndex}";
	        _logger.LogInformation("PrepareQrCode - {Url}", url);
	        
	        var urlPayload = new PayloadGenerator.Url(url);
	        
	        try
	        {
		        using QRCodeGenerator qrGenerator = new QRCodeGenerator();
		        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlPayload);
		        using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
		        var qrCodeBytes = qrCode.GetGraphic(20);
        
		        return Image.Load<Argb32>(qrCodeBytes);
	        }
	        catch (Exception e)
	        {
		        _logger.LogError(e, "Error while Preparing QRCode");
		        return null;
	        }
        }
    }
}
