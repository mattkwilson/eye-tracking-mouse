using System;
using Tobii.StreamEngine;

namespace EyeTrackingMouse.exceptions
{
    public class TobiiStreamEngineException : Exception
    {
        public TobiiStreamEngineException(tobii_error_t e) : base(e.ToString())
        {
            
        }

        public TobiiStreamEngineException(string e) : base(e)
        {
            
        }
    }
}