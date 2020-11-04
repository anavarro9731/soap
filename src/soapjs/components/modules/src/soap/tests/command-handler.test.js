import { mockEvent } from '../command-handler.js';
import {
  ApiQuery,
  ApiCommand,
  TestEvent
} from '../messages.js';
import { commandHandler, queryCache } from '../index.js';
import { md5Hash } from "../util";
import bus from '../bus.js';
import postal from 'postal';

test('queries receive results', () => {
  //- arrange
  const query = Object.assign(new ApiQuery(), { pointlessprop: '12345' });

  let gotIt = false;

    mockEvent(query, [new TestEvent({ results: [new TestEvent.Results({id: 1}), new TestEvent.Results({id: 2})] })]);

  //- listen for response to query
  const conversationId = commandHandler.handle(
    query,
    (result, postalEnvelope) => {
      expect(result instanceof TestEvent).toBe(true);
      expect(result.results[0] instanceof TestEvent.Results).toBe(true);

      if (result.results[0].id === 1) {
        gotIt = true;
      }

    },
    0,
  );
  
  expect(postal.subscriptions.queries[`#.${conversationId}`].length).toBe(1);
  bus.closeConversation(conversationId);

  expect(postal.subscriptions).toStrictEqual({});
  expect(gotIt).toBe(true);
  expect(typeof conversationId).toBe('string');
});

test('queries receive results from cache', () => {
    //- arrange
    const query = Object.assign(new ApiQuery(), { pointlessprop: '12345' });

    let gotIt = false;

    const response = new TestEvent({ results: [new TestEvent.Results({id: 1}), new TestEvent.Results({id: 2})] });
    const queryHash = md5Hash(query);

    queryCache.addOrReplace(queryHash, response);

    //- listen for response to query
    const conversationId = commandHandler.handle(
        query,
        (result, postalEnvelope) => {
            expect(result instanceof TestEvent).toBe(true);
            expect(result.results[0] instanceof TestEvent.Results).toBe(true);

            if (result.results[0].id === 1) {
                gotIt = true;
            }

        },
        5,
    );

    expect(postal.subscriptions).toStrictEqual({});
    expect(gotIt).toBe(true);
    expect(typeof conversationId).toBe('undefined');
});


test('straight commands can receive results', () => {
  //- arrange
  const command = Object.assign(new ApiCommand(), { pointlessprop: '12345' });

  let gotIt = false;

  mockEvent(command, [new TestEvent({ results: [new TestEvent.Results({id: 1}), new TestEvent.Results({id: 2})] })]);

  //- listen for response to query
  const conversationId = commandHandler.handle(
    command,
    (result, postalEnvelope) => {
      if (result.results[0].id === 1) {
        gotIt = true;
      }
    },
    0,
  );

  expect(gotIt).toBe(true);
  expect(typeof conversationId).toBe('string');
});
