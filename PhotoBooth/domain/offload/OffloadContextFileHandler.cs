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

using System.IO;
using System.Linq;
using com.prodg.photobooth.common;
using com.prodg.photobooth.infrastructure.serialization;

namespace com.prodg.photobooth.domain.offload
{
    public class OffloadContextFileHandler : IOffloadContextFileHandler
    {
        private static readonly string FileName = "offloadstatus.json";
        private readonly IStreamSerializer serializer;
        private readonly ILogger logger;

        public OffloadContextFileHandler(IStreamSerializer serializer, ILogger logger)
        {
            this.serializer = serializer;
            this.logger = logger;
        }

        public void Save(OffloadContext context, string folder)
        {
            try
            {
                using (var fileStream = File.OpenWrite(Path.Combine(folder, FileName)))
                {
                    //Load offload status from file
                    serializer.Serialize(fileStream, context);

                }
            }
            catch (IOException e)
            {
                logger.LogException("Error while saving offload context for folder "+folder, e);
            }
        }

        public OffloadContext Load(string folder)
        {
            var context = new OffloadContext();
            try
            {
                var contextFile = Path.Combine(folder, FileName);
                if (File.Exists(contextFile))
                {
                    using (var fileStream = File.OpenRead(contextFile))
                    {
                        //Load offload status from file
                        context = serializer.Deserialize<OffloadContext>(fileStream);
                    }
                }

            }
            catch (IOException e)
            {
                logger.LogException("Error while loading offload context for folder " + folder, e);
            }
            return context;
        }
    }
}
