namespace Soap.Utility.Objects.Blended
{
    using System;

    public class ErrorCode : Enumeration<ErrorCode>
    {
        public static ErrorCode Create(Guid code, string messageSafeForInternalAndExternalClients) =>
            Create(code.ToString(), messageSafeForInternalAndExternalClients);

        public static new ErrorCode Create(string guid, string messageSafeForInternalAndExternalClients) =>
            //HACK hide the base method with it's confusing argument names
            Enumeration<ErrorCode>.Create(Guid.Parse(guid).ToString(), messageSafeForInternalAndExternalClients);

        public override string ToString() => $"{Key}";
    }
}