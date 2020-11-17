import React from 'react';
import 'sanitize.css/sanitize.css';
import * as ServiceBus from '@azure/service-bus/browser/service-bus';
import {
    getListOfRegisteredMessages,
    getRegisteredMessageType,
    registerMessageTypes
} from "@soap/modules/lib/soap/messages";


const config = {
    log(msg) {
        console.log(msg);
    },
    async setup(sender) {
        const fr = process.env.FUNCTIONAPP_ROOT;
        const sr = process.env.SERVICEBUS_ROOT;
        const qn = process.env.SERVICEBUS_QUEUE;

        this.sender = msg => sender(msg, sr, qn);

        const endpoint = `${fr}api/GetJsonSchema`;

        const jsonArrayOfMessages = await global.fetch(endpoint)
            .then(response => response.json());

        registerMessageTypes(jsonArrayOfMessages);
        console.log(getListOfRegisteredMessages());
        const x = getRegisteredMessageType("Enumeration");
        const y = new x({});

    },
    send(message) {
        sender(message);
    }
};

(async function () {

    await config.setup(async (msg, connectionString, queueName) => {

        const sbClient = ServiceBus.ServiceBusClient.createFromConnectionString(connectionString);

    });

    async function sendMessage(sbClient, command) {

        const queueClient = sbClient.createQueueClient(command.headers.queueName);
        const sender = queueClient.createSender();

        const message = {
            body: command,
            label: command.headers.schema,
            sessionId: command.headers.conversationId
        };

        console.log(`Sending message ${command.headers.schema} id/conversation ${command.headers.conversationId}`, command);

        await sender.send(message);
        await queueClient.close();
        
        console.log(`Sent message ${command.headers.messageId}`);

    }
    
    async function receiveMessages(sbClient, sessionId) {

        const queueClient = sbClient.createSubscriptionClient("BrowserClients");
        const receiver = queueClient.createReceiver(ServiceBus.ReceiveMode.peekLock, {
            sessionId: sessionId
        });

        const onMessage = async (brokeredMessage) => {
            console.log(`Received: ${brokeredMessage.sessionId} - ${brokeredMessage.body} `);
        };
        const onError = (err) => {
            console.log(">>>>> Error occurred: ", err);
        };
        receiver.registerMessageHandler(onMessage, onError);
        await delay(5000);

        await queueClient.close();
    }

}());


//ReactDOM.render(<Welcome/>, document.getElementById('content'));
