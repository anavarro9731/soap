namespace Soap.Api.Sample.Afs
{
    using System;

    public interface HelperFunctions
    {
        internal static void SetConfigIdEnvVars()
        {
            Environment.SetEnvironmentVariable(nameof(ConfigId.SoapApplicationKey), "SAP");
            
            var runningInDev = Environment.UserInteractive;
            if (runningInDev)
            {
                Environment.SetEnvironmentVariable(nameof(ConfigId.SoapEnvironmentKey), "DEV");
                Environment.SetEnvironmentVariable(nameof(ConfigId.AzureDevopsOrganisation), "anavarro9731");
                Environment.SetEnvironmentVariable(
                    nameof(ConfigId.AzureDevopsPat),
                    "7ii2qmaehovdujwjgblveash2zc5lc2sqnirjc5f62hkdrdqhwzq");
            }
        }
    }
}