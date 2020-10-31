import { mockEvent } from '../command-handler.js';
import {
  ApiQuery,
  ApiCommand,
  TestEvent,
  TestEvent_Results,
} from '../messages.js';
import { commandHandler } from '../index.js';
import bus from '../bus.js';
import postal from 'postal';

test('queries receive results', () => {
  //- arrange
  const query = Object.assign(new ApiQuery(), { pointlessprop: '12345' });

  let gotIt = false;

  mockEvent(query, [new TestEvent([1, 2])]);

  //- listen for response to query
  const conversationId = commandHandler.handle(
    query,
    (result, postalEnvelope) => {
      expect(result instanceof TestEvent).toBe(true);
      expect(result.resultIds[0] instanceof TestEvent_Results).toBe(true);

      if (result.resultIds[0].id === 1) {
        gotIt = true;
      }
      const x = result.notexist;
      //result.notexist = 1;
    },
    0,
  );

  expect(postal.subscriptions.queries[`*.${conversationId}`].length).toBe(1);
  bus.closeConversation(conversationId);

  expect(postal.subscriptions).toStrictEqual({});
  expect(gotIt).toBe(true);
  expect(typeof conversationId).toBe('string');
});

test('straight commands can receive results', () => {
  //- arrange
  const command = Object.assign(new ApiCommand(), { pointlessprop: '12345' });

  let gotIt = false;

  mockEvent(command, [new TestEvent([1, 2])]);

  //- listen for response to query
  const conversationId = commandHandler.handle(
    command,
    (result, postalEnvelope) => {
      if (result.resultIds[0].id === 1) {
        gotIt = true;
      }
    },
    0,
  );

  expect(gotIt).toBe(true);
  expect(typeof conversationId).toBe('string');
});
