/*
 * PHOTOBOOTH
 * Copyright 2014 Patrick Bronneberg
 * 
*/

using System;

namespace com.prodg.photobooth.infrastructure.hardware
{

    public class HardwareException : Exception
    {

        public HardwareException()
        {
        }

        public HardwareException(string message)
            : base(message)
        {
        }

    }
}
