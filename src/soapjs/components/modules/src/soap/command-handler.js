import {parseDotNetShortAssemblyQualifiedName, md5Hash, types, uuidv4, validateArgs, getHeader, setHeader} from './util';
import {bus, eventHandler, queryCache} from './index';
import config from './config';
import {createRegisteredTypedMessageInstanceFromAnonymousObject, headerKeys} from './messages.js';

let mockEvents = {};

export function mockEvent(command, correspondingEvents) {
    correspondingEvents = Array.isArray(correspondingEvents)
        ? correspondingEvents
        : [correspondingEvents];

    validateArgs(
        [{command}, types.object],
        [{correspondingEvents}, [types.object]],
    );

    correspondingEvents.forEach(event => {

        addHeaders(command, event);

        //* save to queue
        const commandSchema = parseDotNetShortAssemblyQualifiedName(command.$type).className;
        if (!mockEvents[commandSchema]) {
            mockEvents[commandSchema] = [];
        }
        mockEvents[commandSchema].push(event);

    });
}

function addHeaders(command, event) {

    //* would normally be set with publisher which is out of our control (e.g. server)
    setHeader(event, headerKeys.commandConversationId, "we won't know till later, so we'll replace it later");
    const { className } = parseDotNetShortAssemblyQualifiedName(event.$type);
    setHeader(event, headerKeys.schema, className);
    const {headers, ...payload} = command;
    setHeader(event, headerKeys.commandHash, md5Hash(payload));

}

export function cacheEvent(command, correspondingEvents) {
    correspondingEvents = Array.isArray(correspondingEvents)
        ? correspondingEvents
        : [correspondingEvents];

    validateArgs(
        [{command}, types.object],
        [{correspondingEvents}, [types.object]],
    );

    //* because the queryCache module is a singleton ( making private vars are singletons ) we need to clear cache each test
    queryCache.clear();

    const {headers, ...payload} = command;
    const commandHash = md5Hash(payload);

    correspondingEvents.forEach(event => {

        addHeaders(command, event);

        const typedEventWrappedInProxy = createRegisteredTypedMessageInstanceFromAnonymousObject(event);

        /* TODO at present you can only register and query one event per command, in future this could be changed to allow for multiple responses 
        since that is how the command might behave if it was not cached. however for most scenarios this is OK and changing it would complicate matters
         */
        queryCache.addOrReplace(commandHash, typedEventWrappedInProxy);
    });
}

export default {
    handle: function handle(
        command,
        onResponse,
        acceptableStalenessFactorInSeconds,
    ) {

        validateArgs(
            [{command}, types.object],
            [{onResponse}, types.function],
            [{acceptableStalenessFactorInSeconds}, types.number],
        );

        if (
            foundCachedResults(
                command,
                onResponse,
                acceptableStalenessFactorInSeconds,
            )
        ) {
            return;
        }

        const conversationId = uuidv4();
        
        setupHeaders(command);

        subscribeCallerToEventResponses(command);

        sendCommandToApi(command);

        return conversationId;
        
        function setupHeaders(command) {

            //* set headers on outgoing commands
            setHeader(command, headerKeys.messageId, conversationId);
            setHeader(command, headerKeys.timeOfCreationAtOrigin, new Date().toISOString());
            setHeader(command, headerKeys.commandConversationId, conversationId);
            const {headers, ...payload} = command;
            setHeader(command, headerKeys.commandHash,  md5Hash(payload));
            setHeader(command, headerKeys.identityToken, "TBD");
            const {className, assemblyName} = parseDotNetShortAssemblyQualifiedName(command.$type);
            setHeader(command, headerKeys.queueName, assemblyName);
            setHeader(command, headerKeys.schema, className);
            //* headerKeys.statefulProcessId not used by client side code right now
            //* headersKeys.topic only used by events
            const commandBlob = new Blob( [JSON.stringify(command)] );
            
            if (commandBlob.size > 256000) {
                //* make sure these were provided
                getHeader(command, headerKeys.blobId);
                getHeader(command, headerKeys.sasStorageToken);
            }
        }
        
        function subscribeCallerToEventResponses(command) {
            /* subscribe to response for just a brief moment
            in the event that you send multiple identical commands before the first event response is cached
            you will get multiple subscriptions but that should be ok just a little less performant.
            
            also noteworthy is the fact that you can listen for multiple events if they have different
            schemas and multiple messages of the same schema until the conversation is ended
            commands have a conversationId used to terminate the subscriptions when CloseConversation is called
            */
            
            bus.subscribe(
                bus.channels.events,
                '#', //* wildcard
                onResponse,
                getHeader(command, headerKeys.commandConversationId)
            );
        }

        function foundCachedResults(
            command,
            onResponse,
            acceptableStalenessFactorInSeconds,
        ) {
            if (acceptableStalenessFactorInSeconds > 0) {

                const cacheResult = queryCache.find(
                    command,
                    acceptableStalenessFactorInSeconds,
                );

                config.logger.log('cache result:', cacheResult);

                if (cacheResult) {
                    onResponse(cacheResult, undefined);
                }

                return !!cacheResult;
            }
        }

        function sendCommandToApi(command) {

            const commandSchema = getHeader(command, headerKeys.schema);
            const mockedEventsForCommand = mockEvents[commandSchema];
            const commandConversationId = getHeader(command, headerKeys.commandConversationId);

            if (!!mockedEventsForCommand) {
                attemptToFakeEventResponseFromMockQueue(
                    mockedEventsForCommand,
                    commandConversationId,
                );
            } else {
                config.send(command);
            }

            function attemptToFakeEventResponseFromMockQueue(
                mockedEventsForCommand,
                commandConversationId,
            ) {
                // fake API responses
                mockedEventsForCommand.forEach(anonymousEvent => {
                    setHeader(anonymousEvent, headerKeys.commandConversationId, commandConversationId);
                    eventHandler.handle(anonymousEvent);
                });
            }
        }
    },
};
