﻿#region PhotoBooth - MIT - (c) 2014 Patrick Bronneberg
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

namespace com.prodg.photobooth.infrastructure.command
{
    public enum Command
    {
        #region Generic commands
        /// <summary>
        /// Communication is acknowledged
        /// </summary>
        Acknowledge,

        /// <summary>
        /// Error in processing the communication
        /// </summary>
        Error,
        #endregion

        #region Incoming commands
        /// <summary>
        /// Camera Trigger command
        /// </summary>
        Trigger,

        /// <summary>
        /// Print command
        /// </summary>
        Print,

        /// <summary>
        /// Power on/off command
        /// </summary>
        Power,
        #endregion

        #region Outbound commands
        /// <summary>
        /// Prepara a control
        /// </summary>
        PrepareControl,

        /// <summary>
        /// Release a control
        /// </summary>
        ReleaseControl
        #endregion
    }
}