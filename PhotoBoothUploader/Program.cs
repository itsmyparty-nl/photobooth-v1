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
  
  Copyright 2017 Patrick Bronneberg
*/
#endregion

using System;
using System.Threading;
using com.prodg.photobooth.common;
using com.prodg.photobooth.config;
using com.prodg.photobooth.domain.offload;
using com.prodg.photobooth.infrastructure.serialization;
using ItsMyParty.Photobooth.Api;
using ItsMyParty.Photobooth.Client;

namespace PhotoUploader
{
    class Program
    {
        public static IPhotoboothOffloader Offloader { get; private set; }

        public static ILogger Logger { get; private set; }

        public static ISettings Settings { get; private set; }

        public static bool ExitRequested { get; private set; }

        static void Main(string[] args)
        {
            Logger = new NLogger();
            Logger.LogInfo("[Application Started]");

            Settings = new Settings(Logger);
            if (!Settings.OffloadSessions)
            {
                Logger.LogWarning("Ignoring setting. Offloading sessions anyway");
            }

            var serializer = new JsonStreamSerializer(Logger);
            var config = Configuration.Default;
            config.ApiClient = new ApiClient(Settings.OffloadAddress);
            config.Timeout = 20000;
            var sessionApi = new SessionApiApi(config);
            var shotApi = new ShotApiApi(config);

            var offloadContextFileHandler = new OffloadContextFileHandler(serializer, Logger);
            Offloader = new PhotoboothOffloader(sessionApi, shotApi, Settings, Logger,
                offloadContextFileHandler);

            while (!ExitRequested)
            {
                try
                {
                    Logger.LogInfo("[Offload sweep started]");
                    Offloader.OffloadEvent();
                    Logger.LogInfo("[Offload sweep finished]");
                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    Logger.LogException("Error while offloading data", e);
                    throw;
                }   
            }
            Logger.LogInfo("[Application Exit]");
        }
    }
}
