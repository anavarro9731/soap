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
                    if (Guid.TryParse(validationExceptionError.ErrorCode, out var errorCodeGuid))
                    {
                        AllErrors.Add(
                            (CodePrefixes.INVALID, errorCodeGuid, validationExceptionError.ErrorMessage,
                                validationExceptionError.ErrorMessage));
                    }
                    else
                    {
                        AllErrors.Add(
                            (CodePrefixes.INVALID, null, validationExceptionError.ErrorMessage,
                                validationExceptionError.ErrorMessage));
                    }
                }
            }
            else if (exception is DomainException || exception is ApplicationException)
            {
                //* we may have a message crafted as safe for external clients otherwise use default
                var externalErrorMessage = exception.Message == "#default-message#"
                                               ? context.AppConfig.DefaultExceptionMessage
                                               : exception.Message;

                //*FRAGILE* based on .NET exception syntax
                var internalErrorMessage = exception.InnerException?.ToString().SubstringBefore("--- ");

                var prefix = exception switch
                {
                    ApplicationException _ => CodePrefixes.RAW,
                    DomainException _ => CodePrefixes.GUARD,
                    _ => string.Empty
                };

                AllErrors.Add((prefix, null, externalErrorMessage, internalErrorMessage));
            }
            else if (exception is DomainExceptionWithErrorCode domainExceptionWithErrorCode)
            {
                var internalErrorMessage =
                    $"{domainExceptionWithErrorCode.Error.Key} [{domainExceptionWithErrorCode.Error.Value}]"
                    + domainExceptionWithErrorCode.StackTrace;

                if (GlobalErrorCodes.GetAllInstances().Contains(domainExceptionWithErrorCode.Error))
                {
                    AllErrors.Add(
                        (CodePrefixes.GUARD, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error.Key),
                            context.AppConfig
                                   .DefaultExceptionMessage, //* always consider messages based on code alone to be unsafe to show details for
                            internalErrorMessage));
                }
                else
                {
                    AllErrors.Add(
                        (CodePrefixes.GUARD, (Guid?)Guid.Parse(domainExceptionWithErrorCode.Error?.Key),
                            context.AppConfig
                                   .DefaultExceptionMessage, //* always consider messages based on code alone to be unsafe to show details for
                            internalErrorMessage));

                    ;
                }
            }
            else if (exception is ExceptionHandlingException)
            {
                AllErrors.Add((CodePrefixes.EXWHEX, null, context.AppConfig.DefaultExceptionMessage, exception.ToString()));
            }
            else
            {
                AllErrors.Add((CodePrefixes.CLR, null, context.AppConfig.DefaultExceptionMessage, exception.ToString()));
            }

            var count = 0;
            SummaryOfExternalErrorMessages = AllErrors
                                             .Select(
                                                 x =>
                                                     $"Type:{x.Prefix}---Code:{(x.Code.HasValue ? x.Code.ToString() : "N/A")}---ErrorMessage:{x.ExternalMessage}")
                                             .Aggregate(
                                                 (aggregated, next) =>
                                                     {
                                                     var currentCount = ++count;
                                                     return aggregated + Environment.NewLine
                                                                       + $"Error {currentCount} of {AllErrors.Count}" + next;
                                                     });

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
        public Exception ExceptionThrownToContext()
        {
            return new PipelineException(SummaryOfExternalErrorMessages).Op(
                e => e.KnownErrorCodes = AllErrors.Where(e => e.Code.HasValue).Select(e => e.Code.Value).ToList());
        }

        public class CodePrefixes
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
