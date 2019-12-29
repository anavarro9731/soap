namespace Soap.If.Utility.PureFunctions
{
    using System;
    using Soap.If.Interfaces;

    public static class Guard
    {
        public static void Against(
            Func<bool> unacceptable,
            string errorMessage,
            ErrorMessageSensitivity sensitivity = ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients)
        {
            Against(unacceptable(), errorMessage, sensitivity);
        }

        public static void Against(Func<bool> unacceptable, string externalClientMessage, string internalClientMessage)
        {
            Against(unacceptable(), internalClientMessage, externalClientMessage);
        }

        public static void Against(
            bool unacceptable,
            string errorMessage,
            ErrorMessageSensitivity sensitivity = ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients)
        {
            if (unacceptable)
            {
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients) throw new DomainException(errorMessage);
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly)
                {
                    throw new DomainException("#default-message#", new Exception(errorMessage));
                }
            }
        }

        public static void Against(bool unacceptable, string externalClientMessage, string internalClientMessage)
        {
            if (unacceptable) throw new DomainException(externalClientMessage, new Exception(internalClientMessage));
        }

        public static void Against(Func<bool> unacceptable, ErrorCode error)
        {
            if (unacceptable())
            {
                throw new DomainExceptionWithErrorCode(error);
            }
        }

        public static void Against(bool unacceptable, ErrorCode error)
        {
            if (unacceptable)
            {
                throw new DomainExceptionWithErrorCode(error);
            }
        }
    }
}