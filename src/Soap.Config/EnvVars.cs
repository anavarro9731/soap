namespace Soap.Config
{
    using System;

    public static class EnvVars
    {

        public static class ServicePrincipal
        {
            //* runtime sets local.settings.json hierarchical object structures as env vars with __ between levels
            public static string ClientId => Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}__{nameof(ClientId)}");
            public static string ClientSecret => Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}__{nameof(ClientSecret)}");
            public static string TenantId => Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}__{nameof(TenantId)}");
        }
        
        public static string AzureDevopsPat => Environment.GetEnvironmentVariable(nameof(AzureDevopsPat));

        public static string AzureDevopsOrganisation =>  Environment.GetEnvironmentVariable(nameof(AzureDevopsOrganisation));

        public static string SoapApplicationKey => Environment.GetEnvironmentVariable(nameof(SoapApplicationKey));

        public static string SoapEnvironmentKey => Environment.GetEnvironmentVariable(nameof(SoapEnvironmentKey));
        
        public static string AzureWebJobsServiceBus => Environment.GetEnvironmentVariable(nameof(AzureWebJobsServiceBus));

    }
}