/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

namespace com.prodg.photobooth.infrastructure.command
{
    public enum CommandType
    {
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
