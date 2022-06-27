namespace Soap.Idaam
{
    using System;
    using CircuitBoard;
    using Soap.Utility;

    public class AuthErrorCodes : ErrorCode
    {
        /* Error Codes only need to be mapped if there is front-end logic that might depend on them
         otherwise the default error handling logic will do the job of returning the error message but without a specific code. */

        public static readonly ErrorCode NoApiPermissionExistsForThisMessage = Create(
            Guid.Parse("67dcd6ad-9b29-4b90-b1aa-2e714af68884"),
            "This access token is not valid for this message");

    }
}
