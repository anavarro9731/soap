namespace Soap.Utility
{
    using System;
    using System.Reflection;
    using CircuitBoard;
    using Soap.Utility.Functions.Extensions;

    public class DomainExceptionWithErrorCode : Exception
    {
        public DomainExceptionWithErrorCode(ErrorCode error, string externalMessage = null)
            : base(error.Value)
        {
            ExternalMessage = externalMessage;
            CallingAssembly = Assembly.GetCallingAssembly().GetName().Name;
            Error = error.Op(e => e.Active = true);
        }

        public DomainExceptionWithErrorCode()
        {
        }

        public string CallingAssembly { get; set; }

        public ErrorCode Error { get; set; }

        public string ExternalMessage { get; set; }
    }
}
