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

namespace com.prodg.photobooth.config
{
    /// <summary>
    /// Interface for the photobooth settings
    /// </summary>
    public interface ISettings
    {
        #region Command Messenger part

        /// <summary>
        /// The name of the serial port connection to the hardware e.g. COM1 on Windows or /dev/tty0 on linux
        /// </summary>
        string SerialPortName { get; }

        /// <summary>
        /// The baud rate of the serial port connection to the hardware e.g. 9600
        /// </summary>
        int SerialPortBaudRate { get; }

        /// <summary>
        /// Enable / disable the Data Terminal Ready (DTR) signal of the serial port connection to the hardware
        /// </summary>
        /// <remarks>Data Terminal Ready (DTR) is typically enabled during XON/XOFF software handshaking and
        ///  Request to Send/Clear to Send (RTS/CTS) hardware handshaking, and modem communications.</remarks>
        bool SerialPortDtrEnable { get; }

        #endregion

        #region Image storage

        /// <summary>
        /// The base path for storing images and other related data
        /// </summary>
        string StoragePath { get; }

        #endregion

        #region Collage image processing

        /// <summary>
        /// The grid Width of the collage created by the collage image processer (default: 2)
        /// </summary>
        int CollageGridWidth { get; }

        /// <summary>
        /// The grid Height of the collage created by the collage image processer (default: 2)
        /// </summary>
        int CollageGridHeight { get; }

        /// <summary>
        /// The scaling percentage for input images to be used in the collageby the collage image processer (default: 0.50) 
        /// </summary>
        float CollageScalePercentage { get; }

        /// <summary>
        /// The number of pixels to pad outside of images in the collageby the collage image processer (default: 20) 
        /// </summary>
        int CollagePaddingPixels { get; }

        /// <summary>
        /// The aspect ratio of the collage (width divided by height) (default: 3:2)
        /// </summary>
        double CollageAspectRatio { get; }

        #endregion

		#region Printing
		string PrinterName { get; }

        int PrintMarginTop { get; }
        int PrintMarginLeft { get; }
        int PrintMarginBottom { get; }
        int PrintMarginRight { get; }

        int PrintDurationMs { get; }
		#endregion

        #region Save & Offload
        string EventId { get; }
        bool SaveSessions { get; }
        bool OffloadSessions { get; }
        string OffloadAddress { get; }
        #endregion

        int TriggerDelayMs { get; }
    }
}
