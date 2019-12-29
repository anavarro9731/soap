namespace Soap.If.Utility.PureFunctions
{
    using System;
    using System.Reflection;
    using Soap.If.Interfaces;
    using Soap.If.Utility.PureFunctions.Extensions;

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