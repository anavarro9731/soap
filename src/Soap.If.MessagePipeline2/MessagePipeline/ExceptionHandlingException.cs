﻿namespace Soap.If.MessagePipeline.MessagePipeline
{
    using System;

    public class ExceptionHandlingException : Exception
    {
        public ExceptionHandlingException(Exception innerException)
            : base("Exception Occurred While Handling an Exception", innerException)
        {
        }
    }
}