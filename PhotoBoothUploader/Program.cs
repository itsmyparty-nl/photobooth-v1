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

using com.prodg.photobooth.config;
using com.prodg.photobooth.domain.offload;
using com.prodg.photobooth.infrastructure.serialization;
// using ItsMyParty.Photobooth.Api;
// using ItsMyParty.Photobooth.Client;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth
{
    public class Program
    {
        public static IPhotoboothOffloader Offloader { get; private set; }

        public static ILogger<Program> Logger { get; private set; }

        public static ISettings Settings { get; private set; }

        public static bool ExitRequested { get; set; }

        static void Main(string[] args)
        {
            var factory = new LoggerFactory();
            Logger = new Logger<Program>(factory);
            Logger.LogInformation("[Application Started]");

            Settings = new Settings(new Logger<Settings>(factory));
            if (!Settings.OffloadSessions)
            {
                Logger.LogWarning("Ignoring setting. Offloading sessions anyway");
            }

            var serializer = new JsonStreamSerializer(new Logger<JsonStreamSerializer>(factory));
            // var config = Configuration.Default;
            // config.ApiClient = new ApiClient(Settings.OffloadAddress);
            // config.Timeout = 20000;
            // var sessionApi = new SessionApiApi(config);
            // var shotApi = new ShotApiApi(config);

            // var offloadContextFileHandler =
                // new OffloadContextFileHandler(serializer, new Logger<OffloadContextFileHandler>(factory));
            // Offloader = new PhotoboothOffloader(sessionApi, shotApi, Settings, new Logger<PhotoboothOffloader>(factory),
                // offloadContextFileHandler);
            Offloader = new OffloadStub();

            while (!ExitRequested)
            {
                try
                {
                    Logger.LogInformation("[Offload sweep started]");
                    Offloader.OffloadEvent();
                    Logger.LogInformation("[Offload sweep finished]");
                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    Logger.LogError(e,"Error while offloading data");
                    throw;
                }   
            }
            Logger.LogInformation("[Application Exit]");
        }
    }
}
