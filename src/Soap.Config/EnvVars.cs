namespace Soap.Api.Sample.Afs
{
    using System;

    public static class EnvVars
    {
        public static string AzureDevopsPat => Environment.GetEnvironmentVariable(nameof(AzureDevopsPat));

        public static string AzureDevopsOrganisation =>  Environment.GetEnvironmentVariable(nameof(AzureDevopsOrganisation));

        public static string SoapApplicationKey => Environment.GetEnvironmentVariable(nameof(SoapApplicationKey));

        public static string SoapEnvironmentKey => Environment.GetEnvironmentVariable(nameof(SoapEnvironmentKey));
        
        public static string AzureWebJobsServiceBus => Environment.GetEnvironmentVariable(nameof(AzureWebJobsServiceBus));

    }
}