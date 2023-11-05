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

using com.prodg.photobooth.infrastructure.command;
using com.prodg.photobooth.infrastructure.hardware;
using CommandMessenger.TransportLayer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain
{
    /// <summary>
    /// A single session in which pictures are taken which are processed into a single result image
    /// </summary>
    public class PhotoBoothHost : BackgroundService
    {
        public PhotoBooth PhotoBooth { get; }

        private readonly ILogger<PhotoBoothHost> _logger;
        
        public PhotoBoothHost(PhotoBooth photobooth, ILogger<PhotoBoothHost> logger)
        {
            PhotoBooth = photobooth;
            _logger = logger;

            _logger.LogInformation("Creating PhotoBooth Host");
            this.PhotoBooth.Model.ShutdownRequested += (_, _) => StopAsync(new CancellationToken());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            PhotoBooth.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("PhotoBooth Host running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (TaskCanceledException e)
                {
                    _logger.LogInformation("Execution of PhotoBooth Host cancelled");
                }
            }
            PhotoBooth.Stop();
        }
        
        #region IDisposable Implementation

        private bool _disposed;
        
        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _logger.LogInformation("Disposing PhotoBooth Host");
                if (disposing)
                {
                }
                // clean up any unmanaged objects
                _disposed = true;
            }
        }

        ~PhotoBoothHost()
        {
            Dispose(false);
        }

        #endregion
    }
}

