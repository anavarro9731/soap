namespace Soap.PfBase.Tests
{
    using System;
    using Soap.Interfaces.Messages;

    public static class Extensions
    {
        public static void SetDefaultHeadersForIncomingTestMessages(this ApiMessage message)
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

            if (messageHeaders.GetQueue() == null)
            {
                messageHeaders.SetQueueName("queue name");
            }

            if (message is ApiCommand)
            {

                if (messageHeaders.GetSessionId() == null)
                {
                    messageHeaders.SetSessionId(Guid.NewGuid().ToString());
                }

                if (messageHeaders.GetCommandConversationId() == null)
                {
                    messageHeaders.SetCommandConversationId(Guid.NewGuid());
                }

                if (messageHeaders.GetCommandHash() == null)
                {
                    messageHeaders.SetCommandHash("command hash");
                }
            }

            if (messageHeaders.GetTopic() == null)
            {
                messageHeaders.SetTopic("topic");
            }

            if (messageHeaders.GetSchema() == null)
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
        public const string AccessTokenHeader = "access token";

        public const string IdentityTokenHeader = "identity token";

        public const string ServiceLevelAccessTokenHeader = "service level access token";
    }
}
