namespace Soap.Api.Sample.Afs
{
    using System;

    public interface HelperFunctions
    {
        internal static void SetAppKey()
        {
            Environment.SetEnvironmentVariable(nameof(ConfigId.SoapApplicationKey), "SAP");
        }
    }
}