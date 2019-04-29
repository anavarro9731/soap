namespace Soap.If.MessagePipeline.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class PipelineExceptionMessages
    {
        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public List<(string prefix, Guid? code, string message)> Errors { get; set; } = new List<(string prefix, Guid? code, string message)>();

        public string ExternalErrorMessage { get; set; }

        public Guid MessageId { get; set; }

        public string MessageSchema { get; set; }

        public string SensitiveInformation { get; set; }

        public static PipelineExceptionMessages Create(
            Exception exception,
            IApplicationConfig appConfig,
            IApiMessage message,
            IMapErrorCodesFromDomainToMessageErrorCodes mapErrorCodesFromDomainToMessageErrorCodes)
        {
            var pipelineExceptionMessages = new PipelineExceptionMessages();

            if (exception is ValidationException validationException)
            {
                foreach (var validationExceptionError in validationException.Errors)
                {
                    if (Guid.TryParse(validationExceptionError.ErrorCode, out Guid errorCodeGuid))
                    {
                        pipelineExceptionMessages.Errors.Add((CodePrefixes.SYNTAX, errorCodeGuid, validationExceptionError.ErrorMessage));
                    }
                    else
                    {
                        pipelineExceptionMessages.Errors.Add((CodePrefixes.SYNTAX, null, validationExceptionError.ErrorMessage));
                    }
                }

            }
            else if (exception is DomainException)
            {
                var externalErrorMessage = exception.Message == "#default-message#" ? appConfig.DefaultExceptionMessage : exception.Message;

                pipelineExceptionMessages.Errors.Add((CodePrefixes.DOMAIN, null, externalErrorMessage));

                pipelineExceptionMessages.SensitiveInformation = exception.InnerException?.ToString().SubstringBefore("--- ");
                ;
            }
            else if (exception is DomainExceptionWithErrorCode domainExceptionWithErrorCode)
            {
                if (domainExceptionWithErrorCode.Error.IsGlobal)
                {
                    pipelineExceptionMessages.Errors.Add((CodePrefixes.DOMAIN, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error.Key), domainExceptionWithErrorCode.Error.DisplayName));
                }
                else if (mapErrorCodesFromDomainToMessageErrorCodes == null)
                {

                    pipelineExceptionMessages.Errors.Add((CodePrefixes.DOMAIN, null, appConfig.DefaultExceptionMessage));
                    pipelineExceptionMessages.SensitiveInformation = "No mapper defined in handler for: ";
                }
                else
                {
                    mapErrorCodesFromDomainToMessageErrorCodes.DefineMapper().TryGetValue(domainExceptionWithErrorCode.Error, out var messageErrorCode);

                    if (messageErrorCode != null) // a mapping was setup
                    {
                        pipelineExceptionMessages.Errors.Add((CodePrefixes.DOMAIN, (Guid?)Guid.Parse(messageErrorCode.Key), messageErrorCode.DisplayName));
                    }
                    else
                    {
                        pipelineExceptionMessages.Errors.Add((CodePrefixes.DOMAIN, null, appConfig.DefaultExceptionMessage));
                        pipelineExceptionMessages.SensitiveInformation = "Mapping {domain error, msg error} missing from mapper in handler for: ";
                    }
                }

                pipelineExceptionMessages.SensitiveInformation += "Internal:" + domainExceptionWithErrorCode?.Error + Environment.NewLine
                                                                  + domainExceptionWithErrorCode.ToString().SubstringBefore("--- ");
            }
            else if (exception is ExceptionHandlingException)
            {
                pipelineExceptionMessages.Errors.Add((CodePrefixes.EXWHEX, null, appConfig.DefaultExceptionMessage));
                pipelineExceptionMessages.SensitiveInformation = exception.ToString();
            }
            else
            {
                pipelineExceptionMessages.Errors.Add((CodePrefixes.CLR, null, appConfig.DefaultExceptionMessage));
                pipelineExceptionMessages.SensitiveInformation = exception.ToString();
            }

            pipelineExceptionMessages.ExternalErrorMessage = pipelineExceptionMessages
                                                             .Errors.Select(x => $"{x.prefix}:{x.code}:{x.message}")
                                                             .Aggregate((a, b) => a + Environment.NewLine + b);

            //these two lines will sometimes, but not always be on an outer message depending on where in the pipeline it occurs, no harm to copy here
            pipelineExceptionMessages.MessageId = message.MessageId;
            pipelineExceptionMessages.MessageSchema = message.GetType().FullName;
            pipelineExceptionMessages.ApplicationName = appConfig.ApplicationName;
            pipelineExceptionMessages.EnvironmentName = appConfig.EnvironmentName;

            return pipelineExceptionMessages;
        }

        public Exception ToEnvironmentSpecificError(IApplicationConfig appConfig)
        {
            if (appConfig.ReturnExplicitErrorMessages)
            {
                return new PipelineException(message: ExternalErrorMessage + Environment.NewLine + "ERROR DETAILS" + Environment.NewLine + SensitiveInformation,
                    errors: Errors.Where(e => e.code.HasValue).Select(e => e.code.Value.ToString()).ToList());
            }

            return new Exception(ExternalErrorMessage);
        }

        public class CodePrefixes
        {
            public const string CLR = nameof(CLR);

            public const string DOMAIN = nameof(DOMAIN);

            public const string EXWHEX = nameof(EXWHEX);

            public const string INVALID = nameof(INVALID);

            public const string SYNTAX = nameof(SYNTAX);
        }

        public class PipelineException : Exception
        {
            public PipelineException(string message, List<string> errors)
                : base(message)
            {
                Errors = errors;
            }

            public List<string> Errors { get; set; }
        }
    }
}