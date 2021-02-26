import {ServiceBusClient} from '@azure/service-bus';
import {ApplicationInsights} from '@microsoft/applicationinsights-web'
import eventHandler from "./event-handler";
import {getHeader, optional, setHeader, types, validateArgs} from './util';
import {getListOfRegisteredMessages, headerKeys, registerMessageTypes} from './messages';
import {BlobServiceClient} from "@azure/storage-blob";
import _ from "lodash";
import * as signalR from '@microsoft/signalr';

const isTest = process.env.NODE_ENV === 'test';

const _logger = {
    appInsights : null,
    log: (logMsg, logObject, toAzure) => {

        const targetObject = {};
        Error.captureStackTrace(targetObject);
        targetObject.stack = targetObject.stack.substring(5);

        if (typeof logMsg === types.object) logMsg = logMsg.toString();
        if (typeof logObject === types.object) logObject = JSON.stringify(logObject, null, 2);

        validateArgs(
            [{msg: logMsg}, types.string],
            [{toAzure}, types.boolean, optional]
        );

        logMsg += targetObject.stack;
        if (logObject === undefined) toAzure ? console.warn(logMsg) : console.log(logMsg)
        else toAzure ? console.warn(logMsg, logObject) : console.log(logMsg, logObject);

        if (toAzure && !isTest) {
            
            if (!this.appInsights) {
                this.appInsights = new ApplicationInsights({
                    config: {
                        instrumentationKey: vars().appInsightsKey
                    }
                });
                this.appInsights.loadAppInsights();
            }
            this.appInsights.trackTrace({message: logMsg});
        }
    }
};

let _auth0, _sessionDetails;
const _onLoadedCallbacks = [];
(async function () {
    if (!isTest) { 
        await loadConfigState();
        _onLoadedCallbacks.forEach(c => c());
    }
})();

async function loadConfigState() {
    
    //* load all config state, when this is done the system is ready
    {
        const a = async () => { _auth0 = await registerMessageTypesFromApi(); }
        const b = async () => { _sessionDetails = await createBusSession(receiveMessage); } 
        
        const promises = [a(),b()];
        
        await Promise.all(promises);
    }
    
    async function receiveMessage(processor) {

        const functionAppRoot = vars().functionAppRoot;

        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(functionAppRoot)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        hubConnection.on('eventReceived', async message => {
            const messageObj = JSON.parse(message.substring(3)); //don't let signalr do the serialising or it will use the wrong JSON settings
            await processor(messageObj);
        });

        await hubConnection.start();

        const endpoint = `${functionAppRoot}/AddToGroup?connectionId=${encodeURIComponent(hubConnection.connectionId)}`;

        //* don't wait it will finish before first response
        fetch(endpoint); //this will get us messages matched to our environment partition key

        return hubConnection.connectionId;
    }
    
    async function registerMessageTypesFromApi() {

        const endpoint = `${vars().functionAppRoot}/GetJsonSchema`;
        let auth0Info;
        
        const jsonArrayOfMessages = await global.fetch(endpoint)
            .then(response => {

                const auth0Enabled = (response.headers.get('Auth0-Enabled') == 'true');
                if  (auth0Enabled) {
                    auth0Info = {
                        uiAppId : response.headers.get('Auth0-UI-Application-ClientId'),
                        tenantDomain : response.headers.get('Auth0-Tenant-Domain'),
                        redirectUri: response.headers.get('Auth0-Tenant-Domain'),
                        isAuthenticated : false,
                        accessToken : null,
                        identityToken : null,
                        userName: null
                    };
                }

                return response.json();
            });

        registerMessageTypes(jsonArrayOfMessages);
        _logger.log(`Schema built for ${jsonArrayOfMessages.length} messages:`, getListOfRegisteredMessages());
        
        return auth0Info;
    }

    async function createBusSession(receiver) {

        const serviceBusClient = new ServiceBusClient(vars().serviceBusConnectionString); //* timeout at the default of [5 minutes] of inactivity on the AMQP connection
        //* will hold lock on the session for 5 mins. if the user F5's then you will get a new session id and the old one will die off with the servicebusClient 
        //* after connection timeout since there are no lockRenewals on session or send calls on that client. 
        // on further inspection it may be killed as soon as the WSS connection is lost though not able to verify

        const browserSessionId = await receiver(processMessage);

        return {
            browserSessionId,
            serviceBusClient,
        };

        async function processMessage(message) {

            let anonymousEvent = message;

            try {
                _logger.log(`Received message ${getHeader(message, headerKeys.messageId)}`, anonymousEvent);

                if (_.find(message.headers, h => h.key === headerKeys.blobId)) {
                    //* make the swap
                    anonymousEvent = await downloadMessageBlob(anonymousEvent);
                }
                eventHandler.handle(anonymousEvent);
            } catch (err) {
                _logger.log(`>>>>> Error unpacking message ${message.messageId}, ${err + err.stack}`);
            }
        }

        async function downloadMessageBlob(anonymousEvent) {

            const blobId = getHeader(anonymousEvent, headerKeys.blobId);
            const sasUrl = getSasUrl(anonymousEvent);
            const blobServiceClient = new BlobServiceClient(sasUrl);
            const containerClient = blobServiceClient.getContainerClient("large-messages");
            const blobClient = containerClient.getBlobClient(blobId);
            // Get blob content from position 0 to the end
            // In browsers, get downloaded data by accessing downloadBlockBlobResponse.blobBody
            const downloadBlockBlobResponse = await blobClient.download();
            const downloaded = await blobToString(await downloadBlockBlobResponse.blobBody);
            const blobbedMessage = JSON.parse(downloaded);
            await containerClient.deleteBlob(blobId);
            return blobbedMessage;

            //a helper method used to convert a browser Blob into string.
            async function blobToString(blob) {

                const fileReader = new FileReader();
                return new Promise((resolve, reject) => {
                    fileReader.onloadend = (ev) => {
                        resolve(ev.target.result);
                    };
                    fileReader.onerror = reject;
                    fileReader.readAsText(blob);
                });
            }

        }
    }
}

function sendMessage(msg) {

    if (!_sessionDetails) {
        console.error(`trying to send msg ${msg.$type} before types are loaded. message will be discarded`);
    } else {

        (async function (typedMessage) {

            try {
                await send(typedMessage);

            } catch (e) {
                _logger.log(e);
            }

            async function send(message) {

                const queue = getHeader(message, headerKeys.queueName);
                const sender = _sessionDetails.serviceBusClient.createSender(queue);

                _logger.log(`Sending message ${getHeader(message, headerKeys.schema)} id/conversation ${getHeader(message, headerKeys.messageId)}`, message);

                setHeader(message, headerKeys.sessionId, _sessionDetails.browserSessionId);

                if (_.find(message.headers, h => h.key === headerKeys.blobId)) {
                    const messageBlob = new Blob([JSON.stringify(message)]);
                    await uploadMessageToBlobStorage(message, messageBlob);
                    clearDownMessageProperties(message);
                }

                await sender.sendMessages({
                    body: message,
                    messageId: getHeader(message, headerKeys.messageId),
                    subject: message.$type,
                    sessionId: _sessionDetails.browserSessionId
                });

                _logger.log(`Sent message ${getHeader(message, headerKeys.messageId)} to queue ${queue}`);

                await sender.close();
            }

            function clearDownMessageProperties(message) {
                for (const property in message) {
                    switch (property) {
                        case '$type': //* leave it there for deserialisation
                        case 'headers': //* leave it there for downloading blob
                        case 'validate': //* you added this
                        case 'types': //* you added this
                        case 'constructor': //js default
                            break;
                        default:
                            delete message[property];
                    }
                }
            }

            async function uploadMessageToBlobStorage(message, blob) {

                const blobId = getHeader(message, headerKeys.blobId);
                const sasUrl = getSasUrl(message);
                const blobServiceClient = new BlobServiceClient(sasUrl);
                const containerClient = blobServiceClient.getContainerClient("large-messages");
                const blockBlobClient = containerClient.getBlockBlobClient(blobId);
                const typeClass = "AssemblyQualifiedName";
                const typeString = message.$type;
                const options = {metadata: {typeClass, typeString}};
                const uploadBlobResponse = await blockBlobClient.uploadData(blob, options);
                _logger.log(`Upload block blob ${blobId} successfully`, uploadBlobResponse.requestId);

            }
            
        }(msg));
    }
}

function getSasUrl(message) {

    const sasToken = getHeader(message, headerKeys.sasStorageToken);
    const sasUrl = `${vars().blobStorageUri}${sasToken}`;
    _logger.log("attaching to " + sasUrl);
    return sasUrl;
}

function vars() {
    const functionAppRoot = process.env.FUNCTIONAPP_ROOT;
    functionAppRoot || _logger.log("process.env.FUNCTIONAPP_ROOT not defined check .env file.");
    const appInsightsKey = process.env.APPINSIGHTS_KEY;
    appInsightsKey || _logger.log("process.env.APPINSIGHTS_KEY not defined check .env file.");
    const serviceBusConnectionString = process.env.SERVICEBUS_CONN;
    serviceBusConnectionString || _logger.log("process.env.SERVICEBUS_CONN not defined check .env file.");
    const blobStorageUri = process.env.BLOBSTORAGE_URI;
    blobStorageUri || _logger.log("process.env.BLOBSTORAGE_URI not defined check .env file.");
    const envPartitionKey = process.env.ENVIRONMENT_PARTITION_KEY;
    envPartitionKey || _logger.log("process.env.ENVIRONMENT_PARTITION_KEY not defined check .env file.")
    
    return {
        functionAppRoot,
        audience: functionAppRoot + '/',
        appInsightsKey,
        serviceBusConnectionString,
        blobStorageUri,
        envPartitionKey
    };
}

export default {
    get vars() {
      return vars();  
    },
    get logger() {
        return _logger;
    },
    get isLoaded() {
      return !!_sessionDetails;  
    },
    get auth0() {
        return _auth0;
    },
    onLoaded(callback) {
        _onLoadedCallbacks.push(callback);
    },
    send(message) {
        sendMessage(message);
    },
    logFormDetail: false,
    logClassDeclarations: false,
    debugSystemState:false
};
