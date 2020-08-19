namespace Soap.Api.Sample.Afs
{
    using System;
    using System.Threading.Tasks;
    using global::Sample.Logic;
    using global::Sample.Models.Aggregates;
    using Microsoft.Azure.WebJobs;
    using Soap.PfBase.Api;

    public static class ReceiveMessage
    {
        [FunctionName("Receive")]
        public static async Task RunAsync(
            [ServiceBusTrigger("testqueue1", Connection = "sb-soap-dev")]
            string myQueueItem,
            string messageId)
        {
            var runningInDev = Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached;
            if (runningInDev)
            {
                Environment.SetEnvironmentVariable(nameof(ConfigId.SoapEnvironmentKey), "DEV");
                Environment.SetEnvironmentVariable(nameof(ConfigId.SoapApplicationKey), "SAP");
                Environment.SetEnvironmentVariable(
                    nameof(ConfigId.AzureDevopsOrganisation),
                    "https://anavarro9731@dev.azure.com/anavarro9731/soap.config/_git/soap.config");
                Environment.SetEnvironmentVariable(nameof(ConfigId.AzureDevopsPat), "y6gg7funryd4ffv32s4fugxzqgjpeqz5gl4xi2dftdf7mcb5pkia");
            }
            await Functions.Execute<User>(myQueueItem, new MappingRegistration(), messageId);
        }
    }
}