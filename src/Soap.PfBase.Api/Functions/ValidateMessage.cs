namespace Soap.PfBase.Api.Functions
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Serilog;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    public partial class PlatformFunctions
    {
        public static async Task<IActionResult> ValidateMessage(HttpRequest req)
        {
            ILogger logger = null;
            try
            {
                AzureFunctionContext.CreateLogger(out logger);

                string type = req.Query["type"];
                var t = Type.GetType(type);
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var msg = (ApiMessage)requestBody.FromJson(t, SerialiserIds.ApiBusMessage);

                var errorMessage = string.Empty;
                try
                {
                    msg.Validate();
                }
                catch (ValidationException validationException)
                {
                    foreach (var validationExceptionError in validationException.Errors)
                        //* FRAGILE based on FluentValidation Internal Routine
                        errorMessage += validationExceptionError.ErrorMessage.SubstringAfter(':') + Environment.NewLine;
                    return new OkObjectResult(errorMessage);
                }

                return new OkResult();
            }
            catch (Exception e)
            {
                logger.Error("An Error Occurred Validating The Form", e);
                return new OkObjectResult("An Error Occurred Validating The Form");
            }
        }
    }
}
