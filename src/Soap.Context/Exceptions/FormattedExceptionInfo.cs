namespace Soap.Context.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using Soap.Context.Context;
    using Soap.Utility.Functions.Extensions;

    public class FormattedExceptionInfo
    {
        public FormattedExceptionInfo(Exception exception, ContextWithMessageLogEntry context)
        {
            if (exception is ValidationException validationException)
            {
                foreach (var validationExceptionError in validationException.Errors)
                {
                    string internalMessage = $"{validationExceptionError.PropertyName} : {validationExceptionError.ErrorMessage}";
                    if (Guid.TryParse(validationExceptionError.ErrorCode, out var errorCodeGuid))
                    {
                        AllErrors.Add((ErrorSourceType.INVALID, errorCodeGuid, validationExceptionError.ErrorMessage, internalMessage));
                    }
                    else
                    {
                        AllErrors.Add((ErrorSourceType.INVALID, null, validationExceptionError.ErrorMessage, internalMessage));
                    }
                }
            }
            else if (exception is DomainException)
            {
                if (exception.Message == "#default-message#")
                {
                    AllErrors.Add(
                        (ErrorSourceType.GUARD, null, context.AppConfig.DefaultExceptionMessage,
                            exception.InnerException.ToString()));
                }
                else
                {
                    AllErrors.Add((ErrorSourceType.GUARD, null, exception.Message, exception.ToString()));
                }
            }
            else if (exception is DomainExceptionWithErrorCode domainExceptionWithErrorCode)
            {
                var internalErrorMessage =
                    $"{domainExceptionWithErrorCode.Error.Key} [{domainExceptionWithErrorCode.Error.Value}]"
                    + domainExceptionWithErrorCode.StackTrace;

                AllErrors.Add(
                        (ErrorSourceType.GUARD, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error.Key), 
                            context.AppConfig
                                   .DefaultExceptionMessage, //* always consider messages based on code alone to be unsafe to show details for
                            internalErrorMessage));
                
            }
            else if (exception is ApplicationException)
            {
                AllErrors.Add((ErrorSourceType.RAW, null, context.AppConfig.DefaultExceptionMessage, exception.ToString()));
            }
            else if (exception is ExceptionHandlingException)  //* won't be sent to clients (see MessagePipeline.Execute catch block)
            {
                AllErrors.Add((ErrorSourceType.EXWHEX, null, context.AppConfig.DefaultExceptionMessage, exception.ToString()));
            }
            else
            {
                AllErrors.Add((ErrorSourceType.CLR, null, context.AppConfig.DefaultExceptionMessage, exception.ToString()));
            }

            SummaryOfExternalErrorMessages = AllErrors
                                             .Select(x => x.ExternalMessage)
                                             .Aggregate((aggregated, next) => aggregated + Environment.NewLine + next);

            ApplicationName = context.AppConfig.AppId;
            EnvironmentName = context.AppConfig.Environment.Value;
        }

        public FormattedExceptionInfo()
        {
            //* serialiser
        }

        public List<(string Prefix, Guid? Code, string ExternalMessage, string InternalMessage)> AllErrors { get; set; } =
            new List<(string Prefix, Guid? Code, string ExternalMessage, string InternalMessage)>();

        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public string SummaryOfExternalErrorMessages { get; set; }

        /* think this is really only used in terms of testing against specific error codes.
         Either in testing, or by frontend clients for very exceptional circumstances.
         Ideally a generic error toaster is fine on the front-end. */
        public PipelineException ExceptionThrownToContext()
        {
            return new PipelineException(SummaryOfExternalErrorMessages).Op(
                e => e.KnownErrorCodes = AllErrors.Where(e => e.Code.HasValue).Select(e => e.Code.Value).ToList());
        }

        public class ErrorSourceType
        {
            public const string CLR = nameof(CLR);

            public const string EXWHEX = nameof(EXWHEX);

            public const string GUARD = nameof(GUARD);

            public const string INVALID = nameof(INVALID);

            public const string RAW = nameof(RAW);
        }

        public class PipelineException : Exception
        {
            public PipelineException(string message)
                : base(message)
            {
            }

            public List<Guid> KnownErrorCodes { get; set; }
        }
    }
}
