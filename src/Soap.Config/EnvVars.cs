namespace Soap.Config
{
    using System;

    //* see notes at top of applicationconfig.cs for more info about how these are used
    public static class EnvVars
    {
        public static string AppId => Environment.GetEnvironmentVariable(nameof(AppId));

        public static string AppInsightsInstrumentationKey =>
            Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        public static string FunctionAppHostUrl = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
        
        public static string AzureBusNamespace => Environment.GetEnvironmentVariable(nameof(AzureBusNamespace));

        public static string AzureDevopsOrganisation => Environment.GetEnvironmentVariable(nameof(AzureDevopsOrganisation));

        public static string AzureDevopsPat => Environment.GetEnvironmentVariable(nameof(AzureDevopsPat));

        public static string AzureResourceGroup => Environment.GetEnvironmentVariable(nameof(AzureResourceGroup));

        public static string AzureStorageConnectionString =>
            Environment.GetEnvironmentVariable(nameof(AzureStorageConnectionString));
        
        public static string AzureWebJobsServiceBus => Environment.GetEnvironmentVariable(nameof(AzureWebJobsServiceBus));
        
        public static string AzureSignalRConnectionString => Environment.GetEnvironmentVariable(nameof(AzureSignalRConnectionString));

        public static string CosmosDbAccountName => Environment.GetEnvironmentVariable(nameof(CosmosDbAccountName));

        public static string CosmosDbDatabaseName => Environment.GetEnvironmentVariable(nameof(CosmosDbDatabaseName));

        public static string CosmosDbKey => Environment.GetEnvironmentVariable(nameof(CosmosDbKey));

        public static string SoapEnvironmentKey => Environment.GetEnvironmentVariable(nameof(SoapEnvironmentKey));

        public static class ServicePrincipal
        {
            //* runtime sets local.settings.json hierarchical object structures as env vars with __ between levels
            public static string ClientId => Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}:{nameof(ClientId)}");

            public static string ClientSecret =>
                Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}:{nameof(ClientSecret)}");

            public static string TenantId => Environment.GetEnvironmentVariable($"{nameof(ServicePrincipal)}:{nameof(TenantId)}");
        }
    }
}
