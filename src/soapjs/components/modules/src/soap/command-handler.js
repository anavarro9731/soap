import {parseDotNetShortAssemblyQualifiedName, md5Hash, types, uuidv4, validateArgs} from './util';
import {bus, eventHandler, queryCache} from './index';
import config from './config';
import {createRegisteredTypedMessageInstanceFromAnonymousObject} from './messages.js';
import {convertDotNetAssemblyQualifiedNameToJsClassName} from "../../lib/soap/util";

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
        const commandName = command.$type;  //* gets constructor name
        if (!mockEvents[commandName]) {
            mockEvents[commandName] = [];
        }
        mockEvents[commandName].push(event);

    });
}

function addHeaders(command, event) {
    
    event.headers = {
        commandConversationId: "we won't know till later, so we'll replace it later",
        schema: parseDotNetShortAssemblyQualifiedName(event.$type).className, //- would normally be set with publisher which is out of our control (e.g. server)
    };
    const {headers, ...payload} = command;
    event.headers.commandHash = md5Hash(payload);

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

        /* subscribe to response for just a brief moment
        in the event that you send multiple identical commands before the first event response is cached
        you will get multiple subscriptions but that should be ok just a little less performant.
       
        also noteworthy is the fact that you can listen for multiple events if they have different
        schemas and multiple messages of the same schema until the conversation is ended
        commands have a conversationId used to terminate the subscriptions when CloseConversation is called
        */

        //* set headers
        command.headers.push(makeHeader("messageId", conversationId));
        command.headers.push(makeHeader("timeOfCreationAtOrigin", new Date().toISOString()));
        command.headers.push(makeHeader("commandConversationId", conversationId));
        const {headers, ...payload} = command;
        command.headers.push(makeHeader("commandHash",  md5Hash(payload)));
        command.headers.push(makeHeader("identityToken", "TBD"));
        const {assemblyName, className} = convertDotNetAssemblyQualifiedNameToJsClassName(command.$type);
        command.headers.push(makeHeader("queueName", assemblyName));
        command.headers.push(makeHeader("schema", className));
        //* statefulprocessid not used by client side code right now
        //* topic only used by events
        
        //TODO upload to blob storage if too big
        let tooBig = false;
        let newBlobId = "";
        command.headers.push(makeHeader("blobId", newBlobId));
        
        subscribeCallerToEventResponses(command);

        sendCommandToApi(command);

        return conversationId;

        function makeHeader(name, value) {
            return {key: name, value, active: true};
        }

        function subscribeCallerToEventResponses(command) {
            //- to everything with this conversation id
            bus.subscribe(
                bus.channels.events,
                '#',
                onResponse,
                command.headers.messageId
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

                config.log('cache result:', cacheResult);

                if (cacheResult) {
                    onResponse(cacheResult, undefined);
                }

                return !!cacheResult;
            }
        }

        function sendCommandToApi(command) {

            const commandName = command.headers.schema;
            const mockedEventsForCommand = mockEvents[commandName];
            const commandConversationId = command.headers.messageId;

            if (!!mockedEventsForCommand) {
                attemptToFakeEventResponseFromMockQueue(
                    mockedEventsForCommand,
                    commandConversationId,
                );
            } else {
                config.send(command, commandConversationId);
            }

            function attemptToFakeEventResponseFromMockQueue(
                mockedEventsForCommand,
                commandConversationId,
            ) {
                // fake API responses
                mockedEventsForCommand.forEach(anonymousEvent => {
                    anonymousEvent.headers.commandConversationId = commandConversationId;
                    eventHandler.handle(anonymousEvent);
                });
            }
        }
    },
};
