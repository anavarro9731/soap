namespace Soap.Utility.PureFunctions
{
    using System;
    using System.Reflection;

    public class Guard
    {
        public static void Against(
            Func<bool> unacceptable,
            string errorMessage,
            ErrorMessageSensitivity sensitivity = ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients,
            Guid? code = null)
        {
            Against(unacceptable(), errorMessage, sensitivity, code);
        }

        public static void Against(Func<bool> unacceptable, string externalClientMessage, string internalClientMessage, Guid? code = null)
        {
            Against(unacceptable(), internalClientMessage, externalClientMessage, code);
        }

        public static void Against(
            bool unacceptable,
            string errorMessage,
            ErrorMessageSensitivity sensitivity = ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients,
            Guid? code = null)
        {
            if (unacceptable)
            {
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForExternalAndInternalClients) throw new DomainException(errorMessage, code);
                if (sensitivity == ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly) throw new DomainException(null, new Exception(errorMessage), code);
            }
        }

        public static void Against(bool unacceptable, string externalClientMessage, string internalClientMessage, Guid? code = null)
        {
            if (unacceptable) throw new DomainException(externalClientMessage, new Exception(internalClientMessage), code);
        }
    }

    public class DomainException : Exception
    {
        public DomainException(string message, Guid? code = null)
            : base(message)
        {
            ErrorCode = code;
            CallingAssembly = Assembly.GetCallingAssembly().GetName().Name;
        }

        public DomainException(string message, Exception innerException, Guid? code = null)
            : base(message, innerException)
        {
            ErrorCode = code;
        }

        public string CallingAssembly { get; set; }

        public Guid? ErrorCode { get; set; }
    }
}
