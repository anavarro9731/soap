namespace Palmtree.ApiPlatform.Infrastructure.Models
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
