namespace Soap.Utility.Models
{
    using System;
    using System.Reflection;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public class DomainExceptionWithErrorCode : Exception
    {
        public DomainExceptionWithErrorCode(ErrorCode error)
            : base(null)
        {
            CallingAssembly = Assembly.GetCallingAssembly().GetName().Name;
            Error = error.Op(e => e.Active = true);
        }

        internal DomainExceptionWithErrorCode()
        {
        }

        public string CallingAssembly { get; set; }

        public ErrorCode Error { get; set; }
    }
}