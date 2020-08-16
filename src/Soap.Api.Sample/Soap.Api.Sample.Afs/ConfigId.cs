namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Text;

    public static class ConfigId
    {
        public static string AzureDevopsRepoPath => System.Environment.GetEnvironmentVariable("SoapAzureDevopsRepoPath");

        public static string AzureDevopsRepoPat => System.Environment.GetEnvironmentVariable("SoapAzureDevopsRepoPath");

        public static string Environment => System.Environment.GetEnvironmentVariable("SoapEnvironment");

        public static string ApplicationId => System.Environment.GetEnvironmentVariable("SoapApplicationName");
        
    }
}