namespace Soap.Config
{
    using System;
    using Soap.Interfaces;

    //* see notes at top of applicationconfig.cs for more info about how these are used
    // adding an item here requires in most cases 
    // adding a line to AzureFunctionContext guards
    
    public static class EnvVars
    {
        //* WEBSITE_HOSTNAME set by functions runtime
        public static string FunctionAppHostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

        public static string FunctionAppHostUrlWithTrailingSlash =
            $"{SoapEnvironmentKey switch { var x when x == SoapEnvironments.InMemory.Key => "domain-test", var x when x == SoapEnvironments.Development.Key => "http", _ => "https" }}://{FunctionAppHostName}/api/";

        //*  Not present in DEV, otherwise created by default by function app. If present will log to azure otherwise only to console.
        public static string AppInsightsInstrumentationKey =>
            Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
        
        /* CUSTOM */
        
        public static string AppId => Environment.GetEnvironmentVariable(nameof(AppId));

        public static string AzureBusNamespace => Environment.GetEnvironmentVariable(nameof(AzureBusNamespace));

        public static string AzureDevopsOrganisation => Environment.GetEnvironmentVariable(nameof(AzureDevopsOrganisation));

        public static string AzureDevopsPat => Environment.GetEnvironmentVariable(nameof(AzureDevopsPat));

        public static string AzureResourceGroup => Environment.GetEnvironmentVariable(nameof(AzureResourceGroup));

        public static string AzureSignalRConnectionString =>
            Environment.GetEnvironmentVariable(nameof(AzureSignalRConnectionString));

        public static string AzureWebJobsServiceBus => Environment.GetEnvironmentVariable(nameof(AzureWebJobsServiceBus));

        public static string AzureWebJobsStorage => Environment.GetEnvironmentVariable(nameof(AzureWebJobsStorage));

        public static string CorsOrigin => Environment.GetEnvironmentVariable(nameof(CorsOrigin));

        public static string CosmosDbDatabaseName => Environment.GetEnvironmentVariable(nameof(CosmosDbDatabaseName));

        public static string CosmosDbEndpointUri => Environment.GetEnvironmentVariable(nameof(CosmosDbEndpointUri));

        public static string CosmosDbKey => Environment.GetEnvironmentVariable(nameof(CosmosDbKey));

        //* only applicable in DEV environment
        public static string EnvironmentPartitionKey => Environment.GetEnvironmentVariable("EnvironmentPartitionKey");

        public static string SoapEnvironmentKey => Environment.GetEnvironmentVariable(nameof(SoapEnvironmentKey));
    }
}
