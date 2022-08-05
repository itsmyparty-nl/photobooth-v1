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

using ItsMyParty.Photobooth.Client;
using System.Globalization;
using System.Net;
using com.prodg.photobooth.config;
using ItsMyParty.Photobooth.Api;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain.offload
{
    public class PhotoboothOffloader : IPhotoboothOffloader
    {
        private readonly ISessionApiApi _sessionApi;
        private readonly IShotApiApi _shotApi;
        private readonly long _eventId;
        private readonly string _eventFolder;
        private readonly ILogger<PhotoboothOffloader> _logger;
        private readonly IOffloadContextFileHandler _offloadContextFileHandler;

        public PhotoboothOffloader(ISessionApiApi sessionApi, IShotApiApi shotApi, ISettings settings,
            ILogger<PhotoboothOffloader> logger,
            IOffloadContextFileHandler offloadContextFileHandler)
        {
            _sessionApi = sessionApi;
            _shotApi = shotApi;
            _eventId = settings.ApiEventId;
            _eventFolder = Path.Combine(settings.StoragePath, settings.EventId);
            _logger = logger;
            _offloadContextFileHandler = offloadContextFileHandler;

            //Allow all SSL certificates
            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
        }

        public void OffloadEvent()
        {
            _logger.LogInformation("Sweeping event folder {EventFolder}", _eventFolder);
            foreach (var sessionFolder in Directory.EnumerateDirectories(_eventFolder).OrderBy(f => f))
            {
                OffloadSessionFolder(sessionFolder);
            }
        }

        public void OffloadSession(int sessionIndex)
        {
            try{
                OffloadSessionFolder(Path.Combine(_eventFolder, sessionIndex.ToString(CultureInfo.InvariantCulture)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while offloading session");
            }
        }

        private void OffloadSessionFolder(string sessionFolder)
        {
            var context = _offloadContextFileHandler.Load(sessionFolder);
            try
            {
                var index = Convert.ToInt32(sessionFolder.Replace(_eventFolder+Path.DirectorySeparatorChar, ""));

                SessionDTO session;
                if (context.EventCreated)
                {
                    session = _sessionApi.GetSessionByIndex(_eventId, index);
                }
                else
                {
                    var timestamp = Directory.GetCreationTime(sessionFolder);
                    session = CreateSession(index, timestamp);
                    context.EventCreated = true;
                }


                foreach (var fullFilePath in Directory.GetFiles(sessionFolder, "*.jpg"))
                {
                    if (context.IsShotOffloaded(fullFilePath)) continue;
                    
                    var fullFilename = Path.Combine(sessionFolder, fullFilePath);
                    UploadShot(fullFilename, session, context);
                    GC.Collect();
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

        private SessionDTO CreateSession(int index, DateTime timestamp)
        {
            var session = new SessionDTO {Index = index, Timestamp = timestamp, EventId = _eventId};
            var createdSession = _sessionApi.CreateEventSession(_eventId, session);
            return createdSession;
        }

        private void UploadShot(string fullFilename, SessionDTO session, OffloadContext? context)
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

                _shotApi.CreateEventSessionShot(_eventId, session.Index, shot);
                context.ShotOffloadFinished(fullFilename, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while offloading shot {FullFilename}", fullFilename);
                context.ShotOffloadFinished(fullFilename, false);
                context.Errors.Add(e.Message);
            }
        }

        private static string LoadImageAsBase64(string fullFilename)
        {
            using (FileStream stream = new FileStream(fullFilename, FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return Base64ImageConverter.ToBase64Jpg(ms.ToArray());
                }
            }
        }
    }
}
