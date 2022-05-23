namespace Soap.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;

    public static class Extensions
    {
        public static List<Enumeration> SetMessageId(this List<Enumeration> m, Guid messageId)
        {
            if (!m.Exists(v => v.Key == Keys.MessageId))
            {
                m.Add(new Enumeration(Keys.MessageId, messageId.ToString()));
            }

            return m;
        }

        public static string ToShortAssemblyTypeName(this Type t) => $"{t.FullName}, {t.Assembly.GetName().Name}";
        
        public static List<Enumeration> SetSchema(this List<Enumeration> m, string schema)
        {
            if (!m.Exists(v => v.Key == Keys.Schema))
            {
                m.Add(new Enumeration(Keys.Schema, schema));
            }
            else
            {
                throw new ApplicationException($"Cannot set header {Keys.Schema} because it has already been set");
            }

            return m;
        }

        public static List<Enumeration> SetSessionId(this List<Enumeration> m, string sessionId)
        {
            if (!m.Exists(v => v.Key == Keys.Sessions.SessionId))
            {
                m.Add(new Enumeration(Keys.Sessions.SessionId, sessionId));
            }
            else
            {
                throw new ApplicationException($"Cannot set header {Keys.Sessions.SessionId} because it has already been set");
            }

            return m;
        }

        public static List<Enumeration> SetStatefulProcessId(this List<Enumeration> m, StatefulProcessId id)
        {
            if (!m.Exists(v => v.Key == Keys.StatefulProcessId))
            {
                m.Add(new Enumeration(Keys.StatefulProcessId, id.ToString()));
            }
            else
            {
                throw new ApplicationException($"Cannot set header {Keys.StatefulProcessId} because it has already been set");
            }

            return m;
        }

        public static List<Enumeration> SetTimeOfCreationAtOrigin(this List<Enumeration> m)
        {
            var s = DateTime.UtcNow.ToString("s");
            if (!m.Exists(v => v.Key == Keys.TimeOfCreationAtOrigin))
            {
                m.Add(new Enumeration(Keys.TimeOfCreationAtOrigin, s));
            }
            else
            {
                throw new ApplicationException($"Cannot set header {Keys.TimeOfCreationAtOrigin} because it has already been set");
            }

            return m;
        }
        
                public static List<Enumeration> SetCommandConversationId(this List<Enumeration> m, Guid conversationId)
        {
            if (!m.Exists(v => v.Key == Keys.Sessions.CommandConversationId))
            {
                m.Add(new Enumeration(Keys.Sessions.CommandConversationId, conversationId.ToString()));
            }
            else throw new ApplicationException($"Cannot set header {Keys.Sessions.CommandConversationId} because it has already been set");

            return m;
        }

        public static List<Enumeration> SetCommandHash(this List<Enumeration> m, string commandHash)
        {
            if (!m.Exists(v => v.Key == Keys.Sessions.CommandHash))
            {
                m.Add(new Enumeration(Keys.Sessions.CommandHash, commandHash));
            }            
            else throw new ApplicationException($"Cannot set header {Keys.Sessions.CommandHash} because it has already been set");

            return m;
        }

        public static List<Enumeration> SetIdentityToken(this List<Enumeration> m, string identityToken)
        {
            if (!m.Exists(v => v.Key == Keys.Auth.IdentityToken))
            {
                m.Add(new Enumeration(Keys.Auth.IdentityToken, identityToken));
            }
            else throw new ApplicationException($"Cannot set header {Keys.Auth.IdentityToken} because it has already been set");
            
            return m;
        }
        
        public static List<Enumeration> SetAccessToken(this List<Enumeration> m, string identityToken)
        {
            if (!m.Exists(v => v.Key == Keys.Auth.AccessToken))
            {
                m.Add(new Enumeration(Keys.Auth.AccessToken, identityToken));
            }
            else throw new ApplicationException($"Cannot set header {Keys.Auth.AccessToken} because it has already been set");
            
            return m;
        }
        
        public static List<Enumeration> SetIdentityChain(this List<Enumeration> m, string identityChain)
        {
            if (!m.Exists(v => v.Key == Keys.Auth.IdentityChain))
            {
                m.Add(new Enumeration(Keys.Auth.IdentityChain, identityChain));
            }
            else throw new ApplicationException($"Cannot set header {Keys.Auth.IdentityChain} because it has already been set");
            
            return m;
        }
        
    }

    public class Keys
    {
        //* uniqueId of message
        public const string MessageId = nameof(MessageId);

        //* server side conversation id 
        public const string StatefulProcessId = nameof(StatefulProcessId);

        //* time when message was created / sent
        public const string TimeOfCreationAtOrigin = nameof(TimeOfCreationAtOrigin);

        //* short type name
        internal const string Schema = nameof(Schema);

        internal class Auth
        {
            //* oauth bearer token
            internal const string AccessToken = nameof(AccessToken);

            //* comma separated list of identities that have handled messages in this workflow
            internal const string IdentityChain = nameof(IdentityChain);

            //* openid identity token
            internal const string IdentityToken = nameof(IdentityToken);
        }

        internal class Sessions
        {
            //* id of client side conversation
            internal const string CommandConversationId = nameof(CommandConversationId);

            //* hash of message that started a client side conversation to link it up again (conversationId too specific for cache)
            internal const string CommandHash = nameof(CommandHash);

            //* Session id for web socket client
            internal const string SessionId = nameof(SessionId);
        }
    }
}