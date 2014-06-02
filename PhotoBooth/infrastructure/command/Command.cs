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

namespace com.prodg.photobooth.infrastructure.command
{
    /// <summary>
    /// Commands to be sent / received via the command messenger
    /// </summary>
    /// <remarks>
    /// The order of the enum determines the interface, therefore the order should
    /// never be changed single-sidedly.
    /// </remarks>
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
        /// Prepare a control so that it can be used and displays this state
        /// </summary>
        PrepareControl,

        /// <summary>
        /// Release a control so that it can no longer be used and displays this state
        /// </summary>
        ReleaseControl,
        #endregion

        /// <summary>
        /// Print twice command
        /// </summary>
        PrintTwice,

        /// <summary>
        /// Lock a control so that it can not be used temporarily and displays this state
        /// </summary>
        LockControl,

        /// <summary>
        /// Unlock a control so that it can again be used
        /// </summary>
        UnlockControl
    }
}
