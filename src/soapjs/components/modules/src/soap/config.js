import {ServiceBusClient} from '@azure/service-bus';
import {ApplicationInsights} from '@microsoft/applicationinsights-web'
import eventHandler from "./event-handler";
import {getHeader, optional, setHeader, types, uuidv4, validateArgs} from './util';
import {getListOfRegisteredMessages, headerKeys, registerMessageTypes} from "./messages";

let _appInsights;

let _logger = { log: (logMsg, logObject, toAzure) => {

        if (typeof logMsg === types.object) logMsg = logMsg.toString();
        
        validateArgs(
            [{ msg: logMsg }, types.string],
            [{ toAzure }, types.boolean, optional]
        );

        _appInsights = _appInsights || new ApplicationInsights({
            config: {
                instrumentationKey: process.env.APPINSIGHTS_KEY
            }
        });

        if (logObject === undefined) console.log(logMsg)
        else console.log(logMsg, logObject);

        if (toAzure) {
            _appInsights.loadAppInsights();
            _appInsights.trackTrace({message: logMsg});
        }
    }};

let _sessionDetails;

let _sender = (msg) => {

    (async function (typedMessage, logger) {

        try {

            _sessionDetails = _sessionDetails || await setupSession();

            await send(typedMessage);

            async function setupSession() {

                await registerMessageTypesFromApi();

                return await createEventListener();
                
                async function registerMessageTypesFromApi() {

                    const functionAppRoot = process.env.FUNCTIONAPP_ROOT;
                    const endpoint = `${functionAppRoot}api/GetJsonSchema`;
                    const jsonArrayOfMessages = await global.fetch(endpoint)
                        .then(response => response.json());

                    registerMessageTypes(jsonArrayOfMessages);
                    logger.log(`Schema built for ${jsonArrayOfMessages.length} messages:`, getListOfRegisteredMessages());
                }
                
                async function createEventListener() {

                    const browserSessionId = uuidv4();
                    const serviceBusConnectionString = process.env.SERVICEBUS_CONN;

                    const serviceBusClient = new ServiceBusClient(serviceBusConnectionString); //* timeout at the default of [5 minutes] of inactivity on the AMQP connection
                    //* will hold lock on the session for 5 mins. if the user F5's then you will get a new session id and the old one will die off with the servicebusClient 
                    //* after connection timeout since there are no lockRenewals on session or send calls on that client. 
                    // on further inspection it may be killed as soon as the WSS connection is lost though not able to verify
                    
                    const receiver = await serviceBusClient.acceptSession("allevents", "browserclients", browserSessionId, {receiveMode: "receiveAndDelete"});

                    const processMessage = async (message) => {
                        const anonymousEvent = message.body;
                        try {
                            logger.log(`Received message ${message.messageId} on bus for session: ${message.sessionId}`, anonymousEvent);
                            eventHandler.handle(anonymousEvent);
                        } catch (err) {
                            logger.log(`>>>>> Error unpacking message ${message.messageId}, ${err}`);
                        }

                    };
                    
                    const processError = async (args) => {
                        logger.log(`>>>>> Error receiving message from source ${args.errorSource}`, args.error);
                    };

                    receiver.subscribe({
                        processMessage,
                        processError
                    });

                    return {
                        browserSessionId,
                        serviceBusClient
                    };
                }
            }

            async function send(message) {

                const queue = getHeader(message, headerKeys.queueName);
                const sender = _sessionDetails.serviceBusClient.createSender(queue);

                logger.log(`Sending message ${getHeader(message, headerKeys.schema)} id/conversation ${getHeader(message, headerKeys.messageId)}`, message);

                setHeader(message, headerKeys.sessionId, _sessionDetails.browserSessionId);

                await sender.sendMessages({
                    body: message,
                    messageId: getHeader(message, headerKeys.messageId),
                    subject: getHeader(message, headerKeys.schema),
                    sessionId: _sessionDetails.browserSessionId
                });

                logger.log(`Sent message ${getHeader(message, headerKeys.messageId)}`);
                await sender.close();
            }

        } catch(e) {
            logger.log(e);
        }

    }(msg, _logger));
};


export default {
    set logger(l) {
        _logger = l;
    },
    get logger() {
        return _logger;
    },
    set sender(s) {
        _sender = s;
    },
    send(message) {
        if (_sender === undefined) throw 'sender not defined please set config.sender = (msg) => {};';
        _sender(message);
    }
};
