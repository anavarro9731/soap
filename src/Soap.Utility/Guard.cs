namespace Soap.Utility
{
    using System;
    using CircuitBoard;
    using Soap.Utility.Enums;

    //* dont try to interweave methods in this class, or you will uglify the stacktrace
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
            Against(unacceptable(), externalClientMessage, internalClientMessage);
        }

        public static void Against(
            bool unacceptable,
            string errorMessage,
            ErrorMessageSensitivity sensitivity = ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients)
        {
            if (unacceptable)
            {
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients)
                    throw new DomainException(errorMessage);
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly)
                {
                    throw new DomainException("#default-message#", new Exception(errorMessage));
                }
            }
        }

        public static void Against(bool unacceptable, string internalClientMessage, string externalClientMessage)
        {
            if (unacceptable) throw new DomainException(externalClientMessage, new Exception(internalClientMessage));
        }

        public static void Against(Func<bool> unacceptable, ErrorCode error)
        {
            //* allowing this to be null would throw an error when handling the error, i considered it better to throw it like this 
            if (error == null) throw new CircuitException("Guard was trigger, but Error Code was null");  
            if (unacceptable())
            {
                throw new DomainExceptionWithErrorCode(error);
            }
        }

        public static void Against(bool unacceptable, ErrorCode error, string externalClientMessage = null)
        {
            //* allowing this to be null would throw an error when handling the error, i considered it better to throw it like this
            if (error == null) throw new CircuitException("Guard was trigger, but Error Code was null");
            if (unacceptable)
            {
                throw new DomainExceptionWithErrorCode(error, externalClientMessage);

            }
        }
    }
}
