namespace Soap.Context
{
    using System.Collections.Generic;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public static class AuthFunctions
    {
        public static void AuthoriseMessageOrThrow(ApiMessage message, ApiIdentity identity)
        {
            Guard.Against(!identity.ApiPermissions.Contains(message.GetType().Name), AuthErrorCodes.NoApiPermissionExistsForThisMessage);
        }
    }
}
