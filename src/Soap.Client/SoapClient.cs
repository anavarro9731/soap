namespace Soap.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.ServiceBus;
    using CircuitBoard;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Soap.Interfaces.Messages;

    public class SoapClient
    {
        private const string CommandTypeName = "Soap.Interfaces.Messages.ApiCommand";

        private ServiceBusClient serviceBusClient;

        public async Task<HttpSendResult> Send(string soapHost, object commandToSend, HttpOptions options)
        {
            CheckForCorrectType(commandToSend);

            dynamic
                command = commandToSend; //* user dynamic rather than importing base message library, less dependencies and make code more portable to other languages
            command.Validate(); //* run any user-defined validations on the interface

            List<Enumeration> headers = command.Headers;

            var messageId = Guid.NewGuid();

            SetCommonHeaders(commandToSend, options, headers, messageId);

            var uri = new Uri(
                $"{soapHost.TrimEnd('/')}/api/ReceiveMessageHttp?id={messageId}&type={Uri.EscapeDataString(commandToSend.GetType().ToShortAssemblyTypeName())}");

            var json = SerialiseCommand(commandToSend);

            var httpClient = new HttpClient();

            var result = options.TestMode
                             ? new HttpResponseMessage
                             {
                                 RequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                             }
                             : await httpClient.PostAsync(uri, new StringContent(json));

            return new HttpSendResult
            {
                HttpResponseMessage = result,
                MessageId = messageId,
                JsonSent = json
            };

            void CheckForCorrectType(object command1)
            {
                /* check without needing a reference to ApiCommand type to make porting code easier
                for the most basic way of interacting with Soap endpoints from a 3rd party */

                var t = command1.GetType();
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

                if (!isApiCommand) throw new ArgumentException($"{nameof(command1)} parameter must be of type {CommandTypeName}");
            }
        }

        public async Task<BusSendResult> Send(string busConnectionString, object commandAsObject, BusOptions options)
        {
            if (commandAsObject is ApiCommand command)
            {
                this.serviceBusClient = this.serviceBusClient ?? new ServiceBusClient(busConnectionString);

                command.Validate(); //* run any user-defined validations on the interface

                List<Enumeration> headers = command.Headers;

                var messageId = Guid.NewGuid();

                SetCommonHeaders(command, options, headers, messageId);

                command.Headers.SetQueueName(command.GetType().Assembly.GetName().Name + ".inmemory");
                
                var result = await Send(
                                 command,
                                 options?.BusSessionId,
                                 options?.ScheduleAt,
                                 this.serviceBusClient.CreateSender(command.Headers.GetQueue()),
                                 options);

                return result;

                async Task<BusSendResult> Send(
                    ApiCommand sendCommand,
                    Guid? sessionId,
                    DateTimeOffset? scheduleAt,
                    ServiceBusSender sender,
                    OptionsBase options1)
                {
                    if (sessionId == Guid.Empty)
                        throw new ArgumentOutOfRangeException(
                            $"{nameof(BusOptions.BusSessionId)} cannot be empty guid. Leave it blank or set another Guid");
                    
                    sessionId = sessionId ?? Guid.NewGuid();

                    var serialisedCommand = SerialiseCommand(sendCommand);

                    var queueMessage = CreateBusMessage(sendCommand, serialisedCommand, sessionId.Value);

                    var result1 = new BusSendResult
                    {
                        JsonSent = serialisedCommand,
                        MessageId = sendCommand.Headers.GetMessageId()
                    };

                    if (!options1.TestMode)
                    {
                        if (scheduleAt.HasValue)
                        {
                            var sequenceNumber = await sender.ScheduleMessageAsync(queueMessage, scheduleAt.Value);
                            result1.MessageSequenceNumber = sequenceNumber;
                        }
                        else
                        {
                            await sender.SendMessageAsync(queueMessage);
                        }
                    }

                    return result1;

                    ServiceBusMessage CreateBusMessage(ApiCommand sendCommand1, string messageJson, Guid sessionId1)
                    {
                        /*
                         ******************** IF YOU CHANGE THE LOGIC IN THIS METHOD CHANGE THE COPY IN SOAP.BUS ****************************
                         * it is duplicated because of restrictions on .net target frameworks used in soap.client that prevent importing
                         * of this and other soap pkgs but we need to keep them aligned so things send that way and this are always the same.
                         */

                        var queueMessage1 = new ServiceBusMessage(Encoding.Default.GetBytes(messageJson))
                        {
                            MessageId = sendCommand1.Headers.GetMessageId().ToString(),
                            Subject = sendCommand1.GetType()
                                                  .ToShortAssemblyTypeName(), //* required by clients for quick deserialisation rather than parsing JSON $type
                            CorrelationId = sendCommand1.Headers.GetStatefulProcessId().ToString(),
                            SessionId = sessionId1.ToString()
                        };
                        return queueMessage1;
                    }
                }
            }

            throw new ArgumentException($"Command parameter must be of type {CommandTypeName}");
        }

        private static string SerialiseCommand(object commandToSend)
        {
            /*
             ******************** IF YOU CHANGE THIS METHOD CHANGE THE COPY IN SOAP.BUS ****************************
             * it is duplicated because of restrictions on .net target frameworks used in soap.client that prevent importing
             * of this and other soap pkgs but we need to keep them aligned so things send that way and this are always the same.
             */

            var json = JsonConvert.SerializeObject(
                commandToSend,
                new JsonSerializerSettings
                {
                    DefaultValueHandling =
                        DefaultValueHandling
                            .Include, //* you always want this i think, there could be many reasons subtle errors might be produced, in most cases I think JS will just resolve to undefined which is *mostly* treated all the same but some functions
                    //will error on undefined and not null, in any case its clearer when you can see the fields there (e.g. having default values for FieldData for UICommands) 
                    NullValueHandling =
                        NullValueHandling
                            .Include, //* include them for debugging purposes on outgoing messages, otherwise they would be considered undefined for JS constructors and product the same result, null values are then changed to undefined in JS constructors
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    TypeNameHandling =
                        TypeNameHandling
                            .Objects, //* ideally could be ignored as already known by JS classes, but may affect object graph structure, checking into this means you may be able to make this NONE
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            return json;
        }

        private static void SetCommonHeaders(object commandToSend, OptionsBase options, List<Enumeration> headers, Guid messageId)
        {
            headers.Clear();
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
        }

        public class BusOptions : OptionsBase
        {
            /* when sent by the Soap.Bus this is the "Unit Of Work" id used to track all messages sent in that UOW.
             here we have similar concept. All Messages sent with the same UOW id will be processed in order. */
            public Guid? BusSessionId;

            public DateTimeOffset? ScheduleAt;
        }

        public class BusSendResult : SendResultBase
        {
            public long? MessageSequenceNumber { get; set; }
        }

        public class HttpOptions : OptionsBase
        {
        }

        public class HttpSendResult : SendResultBase
        {
            public HttpResponseMessage HttpResponseMessage { get; set; }
        }

        public abstract class OptionsBase
        {
            public Auth RequiresAuth;

            public SignalRSessionInfo SignalRSession;

            public bool TestMode;
            
            public class Auth
            {
                public Auth(string accessToken, string identityToken, string username)
                {
                    AccessToken = accessToken;
                    IdentityToken = identityToken;
                    Username = username;
                }

                /// <summary>
                ///     Oauth bearer token with rights to perform this action
                /// </summary>
                public string AccessToken { get; }

                /// <summary>
                ///     Openid id token
                /// </summary>
                public string IdentityToken { get; }

                /// <summary>
                ///     IDAAM provider username, usually email address
                /// </summary>
                public string Username { get; }
            }

            public class SignalRSessionInfo
            {
                public SignalRSessionInfo(string commandHash, Guid conversationId, string signalRSessionId)
                {
                    CommandHash = commandHash;
                    ConversationId = conversationId;
                    SignalRSessionId = signalRSessionId;
                }

                /// <summary>
                ///     Events resulting from this command sent back on the websocket conn will be contain this property, it is used for
                ///     caching results
                /// </summary>
                public string CommandHash { get; }

                /// <summary>
                ///     Events resulting from this command sent back on the websocket conn will be tagged with this identifier
                /// </summary>
                public Guid ConversationId { get; }

                /// <summary>
                ///     Session id to be able to send return events
                /// </summary>
                public string SignalRSessionId { get; }
            }
        }

        public abstract class SendResultBase
        {
            public string JsonSent;

            public Guid MessageId;
        }
    }
}