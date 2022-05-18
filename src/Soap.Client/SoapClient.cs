namespace Soap.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CircuitBoard;
    using Newtonsoft.Json;

    public enum Transport
    {
        HttpDirect,

        ServiceBus
    }

    public class SoapClient
    {
        public async Task<SendResult> Send(string soapHost, object command, Options options = null)
        {
            CheckForCorrectType(command);
            options = options ?? new Options();
            switch (options.Transport)
            {
                case Transport.HttpDirect:
                    return await SendByHttpDirect(soapHost, command, options);
                case Transport.ServiceBus:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckForCorrectType(object command)
        {
            const string CommandTypeName = "Soap.Interfaces.Messages.ApiCommand";

            var t = command.GetType();
            var isApiCommand = false;

            while (t != null)
            {
                if (t.FullName == CommandTypeName)
                {
                    isApiCommand = true;
                    break;
                }

                t = t.BaseType;
            }

            if (!isApiCommand) throw new ArgumentException($"{nameof(command)} parameter must be of type {CommandTypeName}");
        }

        private async Task<SendResult> SendByHttpDirect(string soapHost, object commandToSend, Options options)
        {
            dynamic
                x = commandToSend; //* user dynamic rather than importing base message library, less dependencies and make code more portable to other languages
            x.Validate(); //* run any user-defined validations on the interface

            List<Enumeration> headers = x.Headers;

            headers.Clear();

            var messageId = Guid.NewGuid();
            headers.SetMessageId(messageId);

            if (options.SignalRSession != default)
            {
                headers.SetCommandConversationId(options.SignalRSession.ConversationId);
                headers.SetCommandHash(options.SignalRSession.CommandHash);
                headers.SetSessionId(options.SignalRSession.SignalRSessionId);
            }

            if (options.RequiresAuth != default)
            {
                headers.SetIdentityChain("user://" + options.RequiresAuth.Username);
                headers.SetAccessToken(options.RequiresAuth.AccessToken);
                headers.SetIdentityToken(options.RequiresAuth.IdentityToken);
            }

            headers.SetTimeOfCreationAtOrigin();

            headers.SetSchema(commandToSend.GetType().FullName);

            var uri = new Uri($"{soapHost}/api/ReceiveMessageHttp");

            var json = JsonConvert.SerializeObject(commandToSend);

            var httpClient = new HttpClient();

            var result = options.TestMode ? new HttpResponseMessage() : await httpClient.PostAsync(uri, new StringContent(json));

            return new SendResult
            {
                HttpResponseMessage = result,
                MessageId = messageId,
                JsonSent = json
            };
        }

        public class Options
        {
            public Auth RequiresAuth;

            public SignalRSessionInfo SignalRSession;

            public bool TestMode;

            public Transport Transport = Transport.HttpDirect;

            public class Auth
            {
                /// <summary>
                ///     Oauth bearer token with rights to perform this action
                /// </summary>
                public string AccessToken;

                /// <summary>
                ///     Openid id token
                /// </summary>
                public string IdentityToken;

                /// <summary>
                ///     IDAAM provider username, usually email address
                /// </summary>
                public string Username;
            }

            public class SignalRSessionInfo
            {
                /// <summary>
                ///     Events resulting from this command sent back on the websocket conn will be contain this property, it is used for
                ///     caching results
                /// </summary>
                public string CommandHash;

                /// <summary>
                ///     Events resulting from this command sent back on the websocket conn will be tagged with this identifier
                /// </summary>
                public Guid ConversationId;

                /// <summary>
                ///     Session id to be able to send return events
                /// </summary>
                public string SignalRSessionId;
            }
        }

        public class SendResult
        {
            public string JsonSent;

            public Guid MessageId;

            public HttpResponseMessage HttpResponseMessage { get; set; }
        }
    }
}