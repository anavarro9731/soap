import _ from 'lodash';
import * as __ from './util.js';
import { types, validateArgs, uuidv4 } from './util.js';
import { queryCache, eventHandler, bus } from './index';
import config from './config';
import { ApiCommand, ApiQuery, MessageEnvelope } from './messages.js';

let mockEvents = {};

export function mockEvent(commandClass, correspondingEvents) {
  correspondingEvents = Array.isArray(correspondingEvents)
    ? correspondingEvents
    : [correspondingEvents];

  validateArgs(
    [{ commandClass }, types.function],
    [{ correspondingEvents }, [types.object]],
  );

  const commandName = commandClass.name;

  correspondingEvents.forEach(event => {
    const eventEnvelope = new MessageEnvelope(
      event,
      "we won't know till later, so we'll replace it later",
    );

    const commandBusChannel =
      commandClass.prototype instanceof ApiQuery
        ? bus.channels.queries
        : bus.channels.commands;
    eventEnvelope.headers.channel = commandBusChannel; //- would normally be set with publisher which is out of our control (e.g. server)
    /* normally we would want to set eventEnvelope.headers.queryHash = commandHash;
      but querycache is a singleton in essence and we don't want that shared between tests */
    if (!mockEvents[commandName]) {
      mockEvents[commandName] = [];
    }
    mockEvents[commandName].push(eventEnvelope);
  });
}

/*
top-level function: separate process block, use out vars, validate args
nested functions: separate process block if complex
*/

export default {
  handle: function handle(
    command,
    onResponse,
    acceptableStalenessFactorInSeconds,
  ) {
    validateArgs(
      [{ command }, types.object],
      [{ onResponse }, types.function],
      [{ acceptableStalenessFactorInSeconds }, types.number],
    );

    if (
      foundCachedResultsWhenDealingWithAQuery(
        command,
        onResponse,
        acceptableStalenessFactorInSeconds,
      )
    )
      return;

    const conversationId = uuidv4();

    /* subscribe to response for just a brief moment
    in the event that you send multiple identical requests before the first response is cached
    you will get multiple commands/eventsubscriptions but that should be ok just a little less performant.
   
    also noteworthy is the fact that you can listen for multiple messages if they have different
    schemas  and multiple messages of the same schema until the conversation is ended
    all outbound queries have a conversationid which is terminated when CloseConversation is called
    */

    const commandEnvelope = new MessageEnvelope(command, conversationId);

    subscribeCallerToEventResponses(commandEnvelope);

    callApiToTriggerEventResponse(commandEnvelope);

    return conversationId;

    /***********************************************************/

    function subscribeCallerToEventResponses(commandEnvelope) {
      //- everything with this conversation id
      bus.subscribe(
        commandEnvelope.headers.channel,
        '#',
        onResponse,
        commandEnvelope.headers.conversationId,
      );
    }

    function foundCachedResultsWhenDealingWithAQuery(
      command,
      onResponse,
      acceptableStalenessFactorInSeconds,
    ) {
      if (command instanceof ApiQuery) {
        const cacheResult = queryCache.query(
          command,
          acceptableStalenessFactorInSeconds,
        );

        config.log('cache result:', cacheResult);

        if (cacheResult) {
          onResponse(cacheResult.event, undefined);
        }

        return !!cacheResult;
      }
    }

    function callApiToTriggerEventResponse(commandEnvelope) {
      const commandName = commandEnvelope.headers.schema;
      const mockedEventsForCommand = mockEvents[commandName];

      const commandConversationId = commandEnvelope.headers.conversationId;

      if (!!mockedEventsForCommand) {
        attemptToFakeEventResponseFromMockQueue(
          mockedEventsForCommand,
          commandConversationId,
        );
      } else {
        config.getConnected()
          ? config.send(commandEnvelope)
          : config.addToCommandQueue(commandEnvelope);
      }

      function attemptToFakeEventResponseFromMockQueue(
        mockedEventsForCommand,
        commandConversationId,
      ) {
        // fake API responses
        mockedEventsForCommand.forEach(eventEnvelope => {
          eventEnvelope.headers.conversationId = commandConversationId;

          eventHandler.handle(eventEnvelope);
        });
      }
    }
  },
};
