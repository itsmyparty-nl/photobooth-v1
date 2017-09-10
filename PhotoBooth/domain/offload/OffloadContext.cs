﻿#region PhotoBooth - MIT - (c) 2017 Patrick Bronneberg
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

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace com.prodg.photobooth.domain.offload
{
    public class OffloadContext
    {
        [JsonConstructor]
        public OffloadContext()
        {
            ShotsOffloaded = new Dictionary<string, bool>();
            Errors = new List<string>();
        }

        [JsonProperty]
        public bool EventCreated { get; set; }

        [JsonProperty]
        public Dictionary<string, bool> ShotsOffloaded { get; private set; }

        [JsonProperty]
        public List<String> Errors { get; private set; }

        public bool IsShotOffloaded(string fullFilePath)
        {
            var fileName = Path.GetFileName(fullFilePath);  
            return ShotsOffloaded.ContainsKey(fileName) && ShotsOffloaded[fileName];
        }

        public void ShotOffloadFinished(string fullFilePath, bool succeeded)
        {
            var fileName = Path.GetFileName(fullFilePath);
            if (ShotsOffloaded.ContainsKey(fileName))
            {
                ShotsOffloaded.Remove(fileName);
            }
            ShotsOffloaded[fileName] = succeeded;
        }
    }
}
