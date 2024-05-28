#region PhotoBooth - MIT - (c) 2017 Patrick Bronneberg
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

using System.Globalization;
using System.Net;
using com.prodg.photobooth.api;
using com.prodg.photobooth.config;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace com.prodg.photobooth.domain.offload
{
    public class PhotoboothOffloader : IPhotoboothOffloader
    {
        private readonly long _eventId;
        private readonly string _eventFolder;
        private readonly PhotoBoothApiClient _client;
        private readonly ILogger<PhotoboothOffloader> _logger;
        private readonly IOffloadContextFileHandler _offloadContextFileHandler;

        public PhotoboothOffloader(PhotoBoothApiClient client, ISettings settings,
            ILogger<PhotoboothOffloader> logger,
            IOffloadContextFileHandler offloadContextFileHandler)
        {
            
            _logger = logger;
            _eventId = settings.ApiEventId;
            _eventFolder = Path.Combine(settings.StoragePath, settings.EventId);
            _client = client;
            
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append(settings.OffloadAddress.TrimEnd('/')).Append(_client.BaseUrl);
            _client.BaseUrl = urlBuilder.ToString();
            _logger.LogInformation("Offloading configured to URL '{BaseUrl}'", _client.BaseUrl);
            
            _offloadContextFileHandler = offloadContextFileHandler;
        }

        public async Task OffloadEvent()
        {
            _logger.LogInformation("Sweeping event folder {EventFolder}", _eventFolder);
            foreach (var sessionFolder in Directory.EnumerateDirectories(_eventFolder).OrderBy(f => f))
            {
                await OffloadSessionFolder(sessionFolder);
            }
        }

        public async Task OffloadSession(int sessionIndex)
        {
            try{
                await OffloadSessionFolder(Path.Combine(_eventFolder, sessionIndex.ToString(CultureInfo.InvariantCulture)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while offloading session");
            }
        }

        private async Task OffloadSessionFolder(string sessionFolder)
        {
            var context = _offloadContextFileHandler.Load(sessionFolder);
            if (context == null)
            {
                _logger.LogError("Cannot offload, Offload context is not set");
                return;
            }
            try
            {
                var index = Convert.ToInt32(sessionFolder.Replace(_eventFolder+Path.DirectorySeparatorChar, ""));

                SessionDTO session;
                if (context!.EventCreated)
                {
                    session = await _client.SessionsGET2Async(_eventId, index);
                }
                else
                {
                    var timestamp = Directory.GetCreationTime(sessionFolder);
                    session = await CreateSession(index, timestamp);
                    context.EventCreated = true;
                }

                foreach (var fullFilePath in Directory.GetFiles(sessionFolder, "*.jpg"))
                {
                    if (context.IsShotOffloaded(fullFilePath)) continue;
                    
                    await UploadShot(fullFilePath, session, context);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while offloading session {SessionFolder}", sessionFolder);
            }
            finally
            {
                _offloadContextFileHandler.Save(context, sessionFolder);
            }
        }

        private async Task<SessionDTO> CreateSession(int index, DateTime timestamp)
        {
            var session = new SessionDTO {Index = index, Timestamp = timestamp, EventId = _eventId};
           
            return await _client.SessionsPOSTAsync(_eventId, session);
        }

        private async Task UploadShot(string fullFilename, SessionDTO session, OffloadContext? context)
        {
            _logger.LogInformation("Offloading shot {FullFilename}", fullFilename);
            var shotFileName = Path.GetFileName(fullFilename);
            try
            {
                var isCollage = shotFileName.Contains("collage");

                var shot = new ShotDTO
                {
                    SessionId = session.Id,
                    IsCollage = isCollage,
                    Image = LoadImageAsBase64(fullFilename)
                };

                await _client.ShotsPOSTAsync(_eventId, session.Index, shot);
                context?.ShotOffloadFinished(fullFilename, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while offloading shot {FullFilename}", fullFilename);
                context?.ShotOffloadFinished(fullFilename, false);
                context?.Errors.Add(e.Message);
            }
        }

        private static string LoadImageAsBase64(string fullFilename)
        {
            using var image = Image.Load(fullFilename);
            return image.ToBase64String(JpegFormat.Instance);
        }
    }
}
