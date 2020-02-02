namespace Soap.If.MessagePipeline.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class FormattedExceptionInfo 
    {
        public FormattedExceptionInfo(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                foreach (var validationExceptionError in validationException.Errors)
                    if (Guid.TryParse(validationExceptionError.ErrorCode, out var errorCodeGuid))
                    {
                        Errors.Add((CodePrefixes.SYNTAX, errorCodeGuid, validationExceptionError.ErrorMessage));
                    }
                    else
                    {
                        Errors.Add((CodePrefixes.SYNTAX, null, validationExceptionError.ErrorMessage));
                    }
            }
            else if (exception is DomainException)
            {
                var externalErrorMessage = exception.Message == "#default-message#"
                                               ? MContext.AppConfig.DefaultExceptionMessage
                                               : exception.Message;

                Errors.Add((CodePrefixes.DOMAIN, null, externalErrorMessage));

                SensitiveInformation = exception.InnerException?.ToString().SubstringBefore("--- ");
            }
            else if (exception is DomainExceptionWithErrorCode domainExceptionWithErrorCode)
            {
                var mapErrorCodesFromDomainToMessageErrorCodes = MContext.AfterMessageLogEntryObtained.DomainToMessageErrorCodesMapper;
                var errorMessageAppendixWhenNoMapperExists = "Internal:" + domainExceptionWithErrorCode?.Error + Environment.NewLine
                                                             + domainExceptionWithErrorCode.ToString().SubstringBefore("--- ");

                if (domainExceptionWithErrorCode.Error.IsGlobal)
                {
                    Errors.Add(
                        (CodePrefixes.DOMAIN, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error.Key),
                            domainExceptionWithErrorCode.Error.DisplayName));
                }
                else if (mapErrorCodesFromDomainToMessageErrorCodes == null)
                {
                    Errors.Add((CodePrefixes.DOMAIN, null, MContext.AppConfig.DefaultExceptionMessage));
                    SensitiveInformation = $"No mapper defined in handler for: {errorMessageAppendixWhenNoMapperExists}";
                }
                else
                {
                    mapErrorCodesFromDomainToMessageErrorCodes
                        .DefineMapper()
                        .TryGetValue(domainExceptionWithErrorCode.Error, out var messageErrorCode);

                    if (messageErrorCode != null) //- a mapping was setup
                    {
                        Errors.Add((CodePrefixes.DOMAIN, (Guid?)Guid.Parse(messageErrorCode.Key), messageErrorCode.DisplayName));
                    }
                    else
                    {
                        Errors.Add((CodePrefixes.DOMAIN, null, MContext.AppConfig.DefaultExceptionMessage));
                        SensitiveInformation =
                            $"Mapping {{domain error, msg error}} missing from mapper in handler for: {errorMessageAppendixWhenNoMapperExists}";
                    }
                }
            }
            else if (exception is ExceptionHandlingException)
            {
                Errors.Add((CodePrefixes.EXWHEX, null, MContext.AppConfig.DefaultExceptionMessage));
                SensitiveInformation = exception.ToString();
            }
            else
            {
                Errors.Add((CodePrefixes.CLR, null, MContext.AppConfig.DefaultExceptionMessage));
                SensitiveInformation = exception.ToString();
            }

            ExternalErrorMessage = Errors.Select(x => $"{x.prefix}:{x.code}:{x.message}").Aggregate((a, b) => a + Environment.NewLine + b);

            ApplicationName = MContext.AppConfig.ApplicationName;
            EnvironmentName = MContext.AppConfig.EnvironmentName;
        }

        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public List<(string prefix, Guid? code, string message)> Errors { get; set; } = new List<(string prefix, Guid? code, string message)>();

        public string ExternalErrorMessage { get; set; }

        public Guid MessageId { get; set; }

        public string MessageSchema { get; set; }

        public string SensitiveInformation { get; set; }

        public Exception ToEnvironmentSpecificError()
        {
            if (MContext.AppConfig.ReturnExplicitErrorMessages)
            {
                return new PipelineException(
                    ExternalErrorMessage + Environment.NewLine + "ERROR DETAILS" + Environment.NewLine + SensitiveInformation,
                    Errors.Where(e => e.code.HasValue).Select(e => e.code.Value.ToString()).ToList());
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