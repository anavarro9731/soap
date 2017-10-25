namespace Soap.MessagePipeline.Models
{
    using System;
    using FluentValidation;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.PureFunctions;

    public class PipelineExceptionMessages
    {
        public string ApplicationName { get; set; }

        public string EnvironmentName { get; set; }

        public string ErrorCode { get; set; }

        public string ExternalErrorMessage { get; set; }

        public string InternalErrorMessage { get; set; }

        public Guid MessageId { get; set; }

        public string MessageSchema { get; set; }

        public static PipelineExceptionMessages Create(Exception exception, IApplicationConfig appConfig, IApiMessage message)
        {
            var pipelineExceptionMessages = new PipelineExceptionMessages();
            string errorCode;
            if (exception is ValidationException)
            {
                errorCode = $"{CodePrefixes.SYNTAX}:{message.GetType().FullName}";
                pipelineExceptionMessages.ErrorCode = errorCode;

                pipelineExceptionMessages.ExternalErrorMessage = $"{errorCode}: {exception.Message}";
            }
            else if (exception is DomainException)
            {
                errorCode = $"{CodePrefixes.DOMAIN}:{((DomainException)exception).ErrorCode}";
                pipelineExceptionMessages.ErrorCode = errorCode;

                pipelineExceptionMessages.ExternalErrorMessage = $"{errorCode}: {appConfig.DefaultExceptionMessage}";
            }
            else if (exception is ExceptionHandlingException)
            {
                errorCode = CodePrefixes.EXWHEX;
                pipelineExceptionMessages.ErrorCode = errorCode;

                pipelineExceptionMessages.ExternalErrorMessage = appConfig.DefaultExceptionMessage;
            }
            else
            {
                errorCode = $"{CodePrefixes.CLRTYPE}:{exception.GetType().FullName}";
                pipelineExceptionMessages.ErrorCode = errorCode;

                pipelineExceptionMessages.ExternalErrorMessage = appConfig.DefaultExceptionMessage;
            }

            pipelineExceptionMessages.InternalErrorMessage = errorCode + Environment.NewLine + exception;

            //these two lines will sometimes, but not always be on an outer message, no harm to copy here
            pipelineExceptionMessages.MessageId = message.MessageId;
            pipelineExceptionMessages.MessageSchema = message.GetType().FullName;
            pipelineExceptionMessages.ApplicationName = appConfig.ApplicationName;
            pipelineExceptionMessages.EnvironmentName = appConfig.EnvironmentName;

            return pipelineExceptionMessages;
        }

        public Exception ToEnvironmentSpecificError(IApplicationConfig appConfig)
        {
            return new Exception(appConfig.ReturnExplicitErrorMessages ? InternalErrorMessage : ExternalErrorMessage);
        }

        public class CodePrefixes
        {
            public const string CLRTYPE = nameof(CLRTYPE);

            public const string DOMAIN = nameof(DOMAIN);

            public const string EXWHEX = nameof(EXWHEX);

            public const string INVALID = nameof(INVALID);

            public const string SYNTAX = nameof(SYNTAX);
        }
    }
}