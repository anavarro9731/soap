namespace Soap.Api.Sample.Afs
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.PfBase.Api.Functions;

    public static class GetJsonSchema
    {
        [FunctionName("GetJsonSchema")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log) =>
            Functions.GetJsonSchema(log, typeof(C100v1_Ping).Assembly);
    }
}
