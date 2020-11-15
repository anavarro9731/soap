import React from 'react';
import ReactDOM from 'react-dom';
import Welcome from './Welcome.jsx';
import 'sanitize.css/sanitize.css';
import  * as SB from '@azure/service-bus/browser/service-bus';

// ServiceBusClient holds reference to the ServiceBusClient global object exposed by the imported script
const ServiceBusClient = SB.ServiceBusClient;
console.log(SB);
const connectionString = "Endpoint=sb://sb-soapapisample-vnext.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=em2fxApOoneFIqtqJsEqCQkkt294S8UY7vpnpfvyGOI=";
const queueName = "soap.api.sample.messages";

(async function main() {
    // Creates the instance of ServiceBusClient using input connection string
    // Refer to SDK documentation and other Javascript samples for more usage related information
    const sbClient = ServiceBusClient.createFromConnectionString(connectionString);

    try {
        const queueClient = sbClient.createQueueClient(queueName);

        const sender = queueClient.createSender();

        console.log(`Sending message ... `);
        await sender.send({body: "Hello world!"});
console.log("done");
        await queueClient.close();
        
    } finally {
        await sbClient.close();
    }
}());

ReactDOM.render(<Welcome />, document.getElementById('content'));
