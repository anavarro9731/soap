import React from 'react';
import 'sanitize.css/sanitize.css';
import {ServiceBusClient} from '@azure/service-bus';
import {
    getListOfRegisteredMessages,
    getRegisteredMessageType,
    headerKeys,
    registerMessageTypes
} from "@soap/modules/lib/soap/messages";
import {soap} from "@soap/modules";
import {getHeader, uuidv4} from "@soap/modules/lib/soap/util";

const eventHandler = soap.eventHandler;
const config = soap.config;
const commandHandler = soap.commandHandler;
  
config.sender = (msg) => {

        let sessionDetails;

        (async function (typedMessage, logger) {

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
                logger.log(getListOfRegisteredMessages());
                const x = getRegisteredMessageType("C100Ping");

                //* create listener for events from API(s)
                const browserSessionId = "12345";// uuidv4();
                const serviceBusClient = new ServiceBusClient(serviceBusConnectionString); //*  at the default of [5 minutes] of inactivity on the AMQP connection
                //* will hold lock on the session for 5 mins. if the user F5's then you will get a new session id and the old one will die off with the servicebusClient after connection timeout since there are no lockRenewals on session or send calls on that client
                const receiver = await serviceBusClient.acceptSession( "allevents", "browserclients", browserSessionId, {receiveMode: "receiveAndDelete"});
                
                const processMessage = async (message) => {
                    logger.log(`Received: ${message.sessionId} - ${JSON.stringify(message.body)} `);
                    eventHandler.handle(message.body);
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

                await sender.sendMessages({
                    body: message,
                    messageId: getHeader(message, headerKeys.messageId),
                    subject: getHeader(message, headerKeys.schema),
                    sessionId: sessionDetails.browserSessionId
                });

                logger.log(`Sent message ${getHeader(message, headerKeys.messageId)}`);
                await sender.close();
            }
        }(msg, config.logger));
    };

const c100Ping = { $type:'Soap.Api.Sample.Messages.Commands.C100v1Ping, Soap.Api.Sample.Messages', pingedAt: new Date().toISOString(), pingedBy: "aaron", headers : []};
commandHandler.handle(c100Ping, msg => { config.log("GOT IT", msg)}, 0);

//ReactDOM.render(<Welcome/>, document.getElementById('content'));
