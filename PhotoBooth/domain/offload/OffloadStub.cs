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

using Microsoft.Extensions.Logging;

namespace com.prodg.photobooth.domain.offload
{
    public class OffloadStub: IPhotoboothOffloader
    {
        private readonly ILogger<OffloadStub> _logger;

        public OffloadStub(ILogger<OffloadStub> logger)
        {
            _logger = logger;
        }
        
        public Task OffloadEvent()
        {
            return new Task(()=>_logger.LogInformation("Dummy offloading event"));
        }

        public Task OffloadSession(int sessionIndex)
        {
            return new Task(()=>_logger.LogInformation("Dummy offloading session"));
        }
    }
}
