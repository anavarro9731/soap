namespace Soap.Context.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Soap.Context.Context;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Models;

    public class FormattedExceptionInfo
    {
        private readonly ContextWithMessageLogEntry context;

        public FormattedExceptionInfo(Exception exception, ContextWithMessageLogEntry context)
        {
            this.context = context;
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
                                               ? context.AppConfig.DefaultExceptionMessage
                                               : exception.Message;

                Errors.Add((CodePrefixes.DOMAIN, null, externalErrorMessage));

                SensitiveInformation = exception.InnerException?.ToString().SubstringBefore("--- ");
            }
            else if (exception is DomainExceptionWithErrorCode domainExceptionWithErrorCode)
            {
                if (GlobalErrorCodes.GetAll<GlobalErrorCodes>().Contains(domainExceptionWithErrorCode.Error))
                {
                    Errors.Add(
                        (CodePrefixes.DOMAIN, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error.Key),
                            domainExceptionWithErrorCode.Error.Value));
                }
                else 
                {
                    Errors.Add((CodePrefixes.DOMAIN, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error?.Key), context.AppConfig.DefaultExceptionMessage));
                    SensitiveInformation = domainExceptionWithErrorCode.Error + Environment.NewLine + domainExceptionWithErrorCode
                                               .ToString()
                                               .SubstringBefore("--- ");;
                }
            }
            else if (exception is ExceptionHandlingException)
            {
                Errors.Add((CodePrefixes.EXWHEX, null, context.AppConfig.DefaultExceptionMessage));
                SensitiveInformation = exception.ToString();
            }
            else
            {
                Errors.Add((CodePrefixes.CLR, null, context.AppConfig.DefaultExceptionMessage));
                SensitiveInformation = exception.ToString();
            }

            ExternalErrorMessage = Errors.Select(x => $"{x.prefix}:{x.code}:{x.message}")
                                         .Aggregate((a, b) => a + Environment.NewLine + b);

            ApplicationName = context.AppConfig.AppId;
            EnvironmentName = context.AppConfig.Environment.Value;
        }

        public FormattedExceptionInfo()
        {
            //* serialiser
        }

        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public List<(string prefix, Guid? code, string message)> Errors { get; set; } =
            new List<(string prefix, Guid? code, string message)>();

        public string ExternalErrorMessage { get; set; }

        public Guid MessageId { get; set; }

        public string MessageSchema { get; set; }

        public string SensitiveInformation { get; set; }

        public Exception ToEnvironmentSpecificError()
        {
            if (this.context.AppConfig.ReturnExplicitErrorMessages)
            {
                return new PipelineException(
                    ExternalErrorMessage +  Environment.NewLine + Environment.NewLine + 
                    "ERROR DETAILS BELOW" + Environment.NewLine + Environment.NewLine + SensitiveInformation,
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