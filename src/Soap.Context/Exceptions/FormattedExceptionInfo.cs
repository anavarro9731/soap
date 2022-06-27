namespace Soap.Context.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using FluentValidation;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class FormattedExceptionInfo
    {
        public FormattedExceptionInfo(Exception exception, IBootstrapVariables bootstrapVariables)
        {
            if (exception is ValidationException validationException)
            {
                foreach (var validationExceptionError in validationException.Errors)
                {
                    string internalMessage = $"{validationExceptionError.PropertyName} : {validationExceptionError.ErrorMessage}";
                    if (Guid.TryParse(validationExceptionError.ErrorCode, out var errorCodeGuid))
                    {
                        AllErrors.Add((ErrorSourceType.VALIDATOR, errorCodeGuid, validationExceptionError.ErrorMessage, internalMessage));
                    }
                    else
                    {
                        AllErrors.Add((ErrorSourceType.VALIDATOR, null, validationExceptionError.ErrorMessage, internalMessage));
                    }
                }
            }
            else if (exception is DomainException)
            {
                if (exception.Message == "#default-message#")
                {
                    AllErrors.Add(
                        (ErrorSourceType.GUARD, null, bootstrapVariables.DefaultExceptionMessage,
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

                if (string.IsNullOrWhiteSpace(domainExceptionWithErrorCode.ExternalMessage)) {
                {
                    AllErrors.Add(
                        (ErrorSourceType.GUARD, Guid.Parse(domainExceptionWithErrorCode.Error.Key), 
                            bootstrapVariables
                                   .DefaultExceptionMessage, //* always consider messages based on code alone to be unsafe to show details for
                            internalErrorMessage));    
                }}
                else
                {
                    AllErrors.Add(
                        (ErrorSourceType.GUARD, Guid.Parse(domainExceptionWithErrorCode.Error.Key),
                            domainExceptionWithErrorCode.ExternalMessage,
                            internalErrorMessage));
                }
            }
            else if (exception is CircuitException circuitException)
            {
                if (circuitException.Error != null)
                {
                    AllErrors.Add((ErrorSourceType.CIRCUIT, Guid.Parse(circuitException.Error.Key), circuitException.Message, exception.ToString()));
                }
                else
                {
                    AllErrors.Add((ErrorSourceType.CIRCUIT, null, bootstrapVariables.DefaultExceptionMessage, exception.ToString()));    
                }
                
            }
            else if (exception is ExceptionHandlingException)  //* won't be sent to clients (see MessagePipeline.Execute catch block)
            {
                AllErrors.Add((ErrorSourceType.EXWHEX, null, bootstrapVariables.DefaultExceptionMessage, exception.ToString()));
            }
            else
            {
                AllErrors.Add((ErrorSourceType.CLR, null, bootstrapVariables.DefaultExceptionMessage, exception.ToString()));
            }

            SummaryOfExternalErrorMessages = AllErrors
                                             .Select(x => x.ExternalMessage)
                                             .Aggregate((aggregated, next) => aggregated + Environment.NewLine + next);

            ApplicationName = bootstrapVariables.AppId;
            EnvironmentName = bootstrapVariables.Environment.Value;
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
            
            public const string CIRCUIT = nameof(CIRCUIT);

            public const string EXWHEX = nameof(EXWHEX);

            public const string GUARD = nameof(GUARD);

            public const string VALIDATOR = nameof(VALIDATOR);
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
