namespace Soap.PfBase.Tests
{
    using System;
    using Soap.Interfaces.Messages;

    public static class Extensions
    {
        public static void SetDefaultHeadersForIncomingTestMessages(this ApiMessage message, bool authEnabled)
        {
            var messageHeaders = message.Headers;
            
            if (messageHeaders.GetMessageId() == Guid.Empty)
            {
                messageHeaders.SetMessageId(Guid.NewGuid());
            }

            if (messageHeaders.GetTimeOfCreationAtOrigin() == null)
            {
                messageHeaders.SetTimeOfCreationAtOrigin();
            }

            if (message is ApiCommand && authEnabled && string.IsNullOrEmpty(messageHeaders.GetIdentityToken()))
            {
                messageHeaders.SetIdentityToken(TestHeaderConstants.IdentityTokenHeader);
            }
            
            if (string.IsNullOrEmpty(messageHeaders.GetAccessToken()))
            {
                messageHeaders.SetAccessToken(TestHeaderConstants.AccessTokenHeader);
            }

            if (string.IsNullOrEmpty(messageHeaders.GetQueue()))
            {
                messageHeaders.SetQueueName("queue name");
            }
            
            if (message is ApiCommand && string.IsNullOrEmpty(messageHeaders.GetIdentityChain()))
            {
                messageHeaders.SetIdentityChain(TestHeaderConstants.IdentityChainHeader);
            }

            if (string.IsNullOrEmpty(messageHeaders.GetTopic()))
            {
                messageHeaders.SetTopic("topic");
            }
            
            if (messageHeaders.GetSessionId() == null && message is ApiCommand)
            {
                messageHeaders.SetSessionId(Guid.NewGuid().ToString());
            }

            if (messageHeaders.GetCommandConversationId() == null && message is ApiCommand)
            {
                messageHeaders.SetCommandConversationId(Guid.NewGuid());
            }

            if (string.IsNullOrEmpty(messageHeaders.GetCommandHash()) && message is ApiCommand)
            {
                messageHeaders.SetCommandHash("command hash");
            }

            if (string.IsNullOrEmpty(messageHeaders.GetSchema()))
            {
                messageHeaders.SetSchema(message.GetType().FullName);
            }

            /* NOT SET
             BLOBID 
             SASSTORAGETOKEN
             STATEFULPROCESSID
             SESSION IDS
             */
        }
    }

    public static class TestHeaderConstants
    {
        public const string ServiceLevelAccessTokenHeader = "service level access token";
        public const string IdentityTokenHeader = "identity token";
        public const string AccessTokenHeader = "access token";
        public const string IdentityChainHeader = "user://someuser";
    }
}
