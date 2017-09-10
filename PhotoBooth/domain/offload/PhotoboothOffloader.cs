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
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using ItsMyParty.Photobooth.Api;

namespace com.prodg.photobooth.domain.offload
{
    public class PhotoboothOffloader : IPhotoboothOffloader
    {
        private readonly ISessionApiApi sessionApi;
        private readonly IShotApiApi shotApi;
        private readonly long eventId;
        private readonly string eventFolder;
        private readonly ILogger logger;
        private readonly IOffloadContextFileHandler offloadContextFileHandler;

        public PhotoboothOffloader(ISessionApiApi sessionApi, IShotApiApi shotApi, ISettings settings, ILogger logger,
            IOffloadContextFileHandler offloadContextFileHandler)
        {
            this.sessionApi = sessionApi;
            this.shotApi = shotApi;
            this.eventId = settings.ApiEventId;
            this.eventFolder = Path.Combine(settings.StoragePath, settings.EventId);
            this.logger = logger;
            this.offloadContextFileHandler = offloadContextFileHandler;
            
            //Allow all SSL certificates
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public void OffloadEvent()
        {
            logger.LogInfo("Sweeping event folder: " + eventFolder);
            foreach (var sessionFolder in Directory.EnumerateDirectories(eventFolder).OrderBy(f => f))
            {
                OffloadSessionFolder(sessionFolder);
            }
        }

        public void OffloadSession(int sessionIndex)
        {
            OffloadSessionFolder(Path.Combine(eventFolder, sessionIndex.ToString(CultureInfo.InvariantCulture)));
        }

        private void OffloadSessionFolder(string sessionFolder)
        {
            var context = offloadContextFileHandler.Load(sessionFolder);
            try
            {
                var index = Convert.ToInt32(sessionFolder.Replace(eventFolder+Path.DirectorySeparatorChar, ""));

                SessionDTO session;
                if (context.EventCreated)
                {
                    session = sessionApi.GetSessionByIndex(eventId, index);
                }
                else
                {
                    var timestamp = Directory.GetCreationTime(sessionFolder);
                    session = CreateSession(index, timestamp);
                    context.EventCreated = true;
                }


                foreach (var fullFilePath in Directory.GetFiles(sessionFolder, "*.jpg"))
                {
                    if (!context.IsShotOffloaded(fullFilePath))
                    {
                        var fullFilename = Path.Combine(sessionFolder, fullFilePath);
                        UploadShot(fullFilename, session, context);
                        GC.Collect();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogException("Error while offloading session: " + sessionFolder, e);
            }
            finally
            {
                offloadContextFileHandler.Save(context, sessionFolder);
            }
        }

        private SessionDTO CreateSession(int index, DateTime timestamp)
        {
            var session = new SessionDTO() {Index = index, Timestamp = timestamp, EventId = eventId};
            var createdSession = sessionApi.CreateEventSession(eventId, session);
            return createdSession;
        }

        private void UploadShot(string fullFilename, SessionDTO session, OffloadContext context)
        {
            logger.LogInfo("Offloading shot: " + fullFilename);
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

                shotApi.CreateEventSessionShot(eventId, session.Index, shot);
                context.ShotOffloadFinished(fullFilename, true);
            }
            catch (Exception e)
            {
                logger.LogException("Error while offloading shot:" + fullFilename, e);
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
