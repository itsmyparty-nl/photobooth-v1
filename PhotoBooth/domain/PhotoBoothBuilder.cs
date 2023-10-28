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
using com.prodg.photobooth.domain.image;
using com.prodg.photobooth.domain.offload;
using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using CommandMessenger.TransportLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public static class PhotoBoothBuilder
    {

        public static IServiceCollection AddPhotoBooth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISettings, Settings>();

            //Todo: replace this by proper use of config
            var settings = new Settings(configuration);
            AddTransport(services, settings);

            services.AddSingleton<ICommandMessengerTransceiver, CommandMessengerTransceiver>();
            services.AddSingleton<IConsoleCommandReceiver, ConsoleCommandReceiver>();
            services.AddSingleton<IRemoteTriggerService, RemoteTriggerService>();

            services.AddSingleton<IMultiImageProcessor, ImageProcessingChain>();
            AddCamera(services, settings);
            CreatePrinterControls(services, settings);
            services.AddSingleton<IPhotoboothOffloader, PhotoboothOffloader>();
            
            services.AddSingleton<IHardware, Hardware>();
            services.AddSingleton<IPhotoBoothService, PhotoBoothService>();

            services.AddSingleton<IPhotoBoothModel, PhotoBoothModel>();

            services.AddSingleton<PhotoBooth>();

            return services;
        }
        
        private static void AddTransport(IServiceCollection services, ISettings settings)
        {
            // Create Serial Port object
            // Note that for some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.
            if (!string.IsNullOrWhiteSpace(settings.SerialPortName))
            {
                services.AddSingleton<ITransport, SerialPortTransport>();
            }
            else
            {
                services.AddSingleton<ITransport, StubbedTransport>();
            }
        }

        private static void AddCamera(IServiceCollection services, ISettings settings)
        {
            // Create Serial Port object
            // Note that for some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.
            if (!settings.StubCamera)
            {
                services.AddSingleton<ICameraProvider, SharpCameraProvider>();
            }
            else
            {
                services.AddSingleton<ICameraProvider, StubCameraProvider>();
            }
            services.AddSingleton<ICamera, Camera>();
        }
        
        private static void CreatePrinterControls(IServiceCollection services, ISettings settings)
        {
            // Create Serial Port object
            // Note that for some boards (e.g. Sparkfun Pro Micro) DtrEnable may need to be true.
            if (!string.IsNullOrWhiteSpace(settings.PrinterName))
            {
                services.AddSingleton<IPrinter, NetPrinter>();
            }
            else
            {
                services.AddSingleton<IPrinter, PrinterStub>();
            }
        }
    }
}

