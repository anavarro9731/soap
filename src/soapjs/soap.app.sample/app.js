import React from 'react';
import 'sanitize.css/sanitize.css';
import {ServiceBusClient} from '@azure/service-bus';
import {commandHandler, config, eventHandler, getHeader, uuidv4, validateArgs, types, optional } from "@soap/modules";
import {getListOfRegisteredMessages, headerKeys, registerMessageTypes} from "@soap/modules/lib/soap/messages";
import { setHeader } from "../components/modules/src/soap/util";
import {ApplicationInsights} from '@microsoft/applicationinsights-web'

let appInsights;

config.logger = { log: (msg, object, toAzure) => {

    if (typeof msg === types.object) msg = msg.toString();
    validateArgs(
        [{ msg }, types.string],
        [{ object }, types.object, optional],
        [{ toAzure }, types.boolean, optional]
    );

    appInsights = appInsights || new ApplicationInsights({
        config: {
            instrumentationKey: process.env.APPINSIGHTS_KEY
        }
    });

    if (object === undefined) console.log(msg) 
        else console.log(msg, object);
        
    if (toAzure) { 
        appInsights.loadAppInsights();
        appInsights.trackTrace({message: msg});
    }
}};

let sessionDetails;

config.sender = (msg) => {
    
    (async function (typedMessage, logger) {
        
        try {
            
            logger.log("test", {p: 1}, true);
            
            sessionDetails = sessionDetails || await register();

            await send(typedMessage);

            async function register() {

                //* register message types from API
                const functionAppRoot = process.env.FUNCTIONAPP_ROOT;
                const serviceBusConnectionString = process.env.SERVICEBUS_CONN;
                const endpoint = `${functionAppRoot}api/GetJsonSchema`;
                const jsonArrayOfMessages = await global.fetch(endpoint)
                    .then(response => response.json());

                registerMessageTypes(jsonArrayOfMessages);
                logger.log(`Schema built for ${jsonArrayOfMessages.length} messages:`, getListOfRegisteredMessages());

                //* create listener for events from API(s)
                const browserSessionId = uuidv4();
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
                        logger.log(err);
                    }

                };
                const processError = async (args) => {
                    logger.log(`>>>>> Error from error source ${args.errorSource} occurred: `, args.error);
                };

                receiver.subscribe({
                    processMessage,
                    processError
                });

                //* return sessionDetails for use by senders
                return {
                    browserSessionId,
                    serviceBusClient
                };
            }
            
            async function send(message) {

                const queue = getHeader(message, headerKeys.queueName);
                const sender = sessionDetails.serviceBusClient.createSender(queue);

                logger.log(`Sending message ${getHeader(message, headerKeys.schema)} id/conversation ${getHeader(message, headerKeys.messageId)}`, message);

                setHeader(message, headerKeys.sessionId, sessionDetails.browserSessionId);

                await sender.sendMessages({
                    body: message,
                    messageId: getHeader(message, headerKeys.messageId),
                    subject: getHeader(message, headerKeys.schema),
                    sessionId: sessionDetails.browserSessionId
                });

                logger.log(`Sent message ${getHeader(message, headerKeys.messageId)}`);
                await sender.close();
            }

        } catch(e) {
            config.logger.log(e);
        }
        
    }(msg, config.logger));
};

const c100Ping = {
    $type: 'Soap.Api.Sample.Messages.Commands.C100v1Ping, Soap.Api.Sample.Messages',
    pingedAt: new Date().toISOString(),
    pingedBy: "aaron",
    headers: []
};
commandHandler.handle(c100Ping, msg => {
    config.logger.log("Received to original caller", msg)
}, 0);

//ReactDOM.render(<Welcome/>, document.getElementById('content'));
