namespace Soap.Context
{
    using System.Collections.Generic;
    using Soap.Interfaces.Messages;

    public static class AuthFunctions
    {
        public static void AuthoriseMessageOrThrow(ApiMessage message, List<string> permissions)
        {
            Guard.Against(!permissions.Contains(message.GetType().Name), AuthErrorCodes.NoApiPermissionExistsForThisMessage);
        }
    }
}
