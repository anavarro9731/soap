import {ServiceBusClient} from '@azure/service-bus';
import {ApplicationInsights} from '@microsoft/applicationinsights-web'
import eventHandler from "./event-handler";
import {getHeader, optional, setHeader, types, uuidv4, validateArgs} from './util';
import {getListOfRegisteredMessages, headerKeys, registerMessageTypes} from './messages';
import {BlobServiceClient} from "@azure/storage-blob";
import _ from "lodash";


let _appInsights;

let _logger = {
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

        let appInsightsKey;
        if (process) {
            appInsightsKey = process.env.APPINSIGHTS_KEY;    
        }
        appInsightsKey || console.log("process.env.APPINSIGHTS_KEY not defined check .env file.")

        if (!_appInsights) {
            _appInsights = new ApplicationInsights({
                config: {
                    instrumentationKey: appInsightsKey
                }
            });
            _appInsights.loadAppInsights();
        }
        
        logMsg += targetObject.stack;
        if (logObject === undefined) toAzure ? console.warn(logMsg) : console.log(logMsg)
        else toAzure ? console.warn(logMsg, logObject) : console.log(logMsg, logObject);
    
        if (toAzure) {
            _appInsights.trackTrace({message: logMsg});
        }
    }
};

let _sessionDetails;

let _setupReceiver;

const _sender = (msg) => {

    (async function (typedMessage, logger) {

        try {

            _sessionDetails = _sessionDetails || await setupSession();

            await send(typedMessage);

        } catch (e) {
            logger.log(e);
        }

        async function send(message) {

            const queue = getHeader(message, headerKeys.queueName);
            const sender = _sessionDetails.serviceBusClient.createSender(queue);

            logger.log(`Sending message ${getHeader(message, headerKeys.schema)} id/conversation ${getHeader(message, headerKeys.messageId)}`, message);

            setHeader(message, headerKeys.sessionId, _sessionDetails.browserSessionId);

            if (_.find(message.headers, h => h.key == headerKeys.blobId)) {
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

            logger.log(`Sent message ${getHeader(message, headerKeys.messageId)} to queue ${queue}`);

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
            const containerClient = blobServiceClient.getContainerClient("content");
            const blockBlobClient = containerClient.getBlockBlobClient(blobId);
            const typeClass = "AssemblyQualifiedName";
            const typeString = message.$type;
            const options = {metadata: {typeClass, typeString}};
            const uploadBlobResponse = await blockBlobClient.uploadData(blob, options);
            logger.log(`Upload block blob ${blobId} successfully`, uploadBlobResponse.requestId);

        }

        
        function getSasUrl(message) {
            const blobStorageUri = process.env.BLOBSTORAGE_URI;
            blobStorageUri || logger.log("process.env.BLOBSTORAGE_URI not defined check .env file.")
            const sasToken = getHeader(message, headerKeys.sasStorageToken);
            const sasUrl = `${blobStorageUri}${sasToken}`;
            logger.log("attaching to " + sasUrl);
            return sasUrl;
        }

        async function processMessage(message) {

            let anonymousEvent = message;

            try {
                logger.log(`Received message ${getHeader(message, headerKeys.messageId)}`, anonymousEvent);

                if (_.find(message.headers, h => h.key == headerKeys.blobId)) {
                    //* make the swap
                    anonymousEvent = await downloadMessageBlob(anonymousEvent);
                }
                eventHandler.handle(anonymousEvent);
            } catch (err) {
                logger.log(`>>>>> Error unpacking message ${message.messageId}, ${err + err.stack}`);
            }
        }

        async function downloadMessageBlob(anonymousEvent) {

            const blobId = getHeader(anonymousEvent, headerKeys.blobId);
            const sasUrl = getSasUrl(anonymousEvent);
            const blobServiceClient = new BlobServiceClient(sasUrl);
            const containerClient = blobServiceClient.getContainerClient("content");
            const blobClient = containerClient.getBlobClient(blobId);
            // Get blob content from position 0 to the end
            // In browsers, get downloaded data by accessing downloadBlockBlobResponse.blobBody
            const downloadBlockBlobResponse = await blobClient.download();
            const downloaded = await blobToString(await downloadBlockBlobResponse.blobBody);
            const blobbedMessage = JSON.parse(downloaded);
            await containerClient.deleteBlob(blobId);
            return blobbedMessage;

        }

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

        async function registerMessageTypesFromApi() {

            const functionAppRoot = process.env.FUNCTIONAPP_ROOT;
            functionAppRoot || logger.log("process.env.FUNCTIONAPP_ROOT not defined check .env file.")
            const endpoint = `${functionAppRoot}GetJsonSchema`;
            const jsonArrayOfMessages = await global.fetch(endpoint)
                .then(response => response.json());

            registerMessageTypes(jsonArrayOfMessages);
            logger.log(`Schema built for ${jsonArrayOfMessages.length} messages:`, getListOfRegisteredMessages());
        }

        async function setupSession() {

            await registerMessageTypesFromApi();
            
            return await createBusSession();
        }

        async function createBusSession() {

            
            const serviceBusConnectionString = process.env.SERVICEBUS_CONN;
            serviceBusConnectionString || logger.log("process.env.SERVICEBUS_CONN not defined check .env file.")

            const serviceBusClient = new ServiceBusClient(serviceBusConnectionString); //* timeout at the default of [5 minutes] of inactivity on the AMQP connection
            //* will hold lock on the session for 5 mins. if the user F5's then you will get a new session id and the old one will die off with the servicebusClient 
            //* after connection timeout since there are no lockRenewals on session or send calls on that client. 
            // on further inspection it may be killed as soon as the WSS connection is lost though not able to verify

            const browserSessionId = await _setupReceiver(processMessage);
            
            return {
                browserSessionId,
                serviceBusClient,
            };
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
    set receiver(r) {
        _setupReceiver = r;
    },
    send(message) {
        _sender(message);
    },
    logFormDetail: false,
    logClassDeclarations: false
};
