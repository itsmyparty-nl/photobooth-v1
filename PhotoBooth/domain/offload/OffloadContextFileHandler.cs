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

using com.prodg.photobooth.infrastructure.serialization;
using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain.offload
{
    public class OffloadContextFileHandler : IOffloadContextFileHandler
    {
        private static readonly string _fileName = "offloadstatus.json";
        private readonly IStreamSerializer _serializer;
        private readonly ILogger<OffloadContextFileHandler> _logger;

        public OffloadContextFileHandler(IStreamSerializer serializer, ILogger<OffloadContextFileHandler> logger)
        {
            _serializer = serializer;
            _logger = logger;
        }

        public void Save(OffloadContext? context, string folder)
        {
            try
            {
                using var fileStream = File.OpenWrite(Path.Combine(folder, _fileName));
                //Load offload status from file
                _serializer.Serialize(fileStream, context);
            }
            catch (IOException e)
            {
                _logger.LogError(e, "Error while saving offload context for {Folder}",folder);
            }
        }

        public OffloadContext? Load(string folder)
        {
            var context = new OffloadContext();
            try
            {
                var contextFile = Path.Combine(folder, _fileName);
                if (File.Exists(contextFile))
                {
                    using var fileStream = File.OpenRead(contextFile);
                    //Load offload status from file
                    context = _serializer.Deserialize<OffloadContext>(fileStream);
                }
            }
            catch (IOException e)
            {
                _logger.LogError(e, "Error while loading offload context for {Folder} ", folder);
            }
            return context;
        }
    }
}
