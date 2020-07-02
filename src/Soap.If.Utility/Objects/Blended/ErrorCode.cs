namespace Soap.Utility.Objects.Blended
{
    using System;

    public class ErrorCode : Enumeration
    {
        public static T Create<T>(Guid code, string messageSafeForInternalAndExternalClients) where T : ErrorCode, new()
        {
            return Create<T>(code.ToString(), messageSafeForInternalAndExternalClients);
        }

        public static new T Create<T>(string guid, string messageSafeForInternalAndExternalClients) where T : Enumeration, new()
        {
            //HACK hide the base method with it's confusing argument names
            return Enumeration.Create<T>(Guid.Parse(guid).ToString(), messageSafeForInternalAndExternalClients);
        }

        public bool IsGlobal { get; set; }

    }
}