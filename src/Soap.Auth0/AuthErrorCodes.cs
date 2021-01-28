namespace Soap.Context
{
    using System;

    public class AuthErrorCodes : ErrorCode
    {
        /* Error Codes only need to be mapped if there is front-end logic that might depend on them
         otherwise the default error handling logic will do the job of returning the error message but without a specific code. */

        public static readonly ErrorCode NoApiPermissionExistsForThisMessage = Create(
            Guid.Parse("36312a82-ca04-4b09-978f-5bb9e2809c2d"),
            "This access token is not valid for this message");
    }
}
