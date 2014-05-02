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
using System;

namespace com.prodg.photobooth.config
{
    public interface ISettings
    {
        #region Command Messenger part

        string SerialPortName { get; }

        int SerialPortBaudRate { get; }

        bool SerialPortDtrEnable { get; }

        #endregion

        #region Image storage

        string StoragePath { get; }

        #endregion

        #region Image processing

        int CollageGridWidth { get; }

        int CollageGridHeight { get; }

        float CollageScalePercentage { get; }

        int CollagePaddingPixels { get; }

        #endregion
    }
}
