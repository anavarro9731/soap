namespace Soap.PfBase.Api
{
    using System;

    public class Result
    {
        public bool Success;

        public Exception UnhandledError { get; set; }
    }
}