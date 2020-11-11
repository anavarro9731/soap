import {mockEvent, default as commandHandler, cacheEvent} from '../command-handler';
import {TestEvent_e200v1, TestCommand_c100v1 } from './test-messages';
import { defineTestMessages } from './test-message-utils';
import bus from '../bus';
import postal from 'postal';
import {getRegisteredMessageType, } from "../messages";

test('commands can receive receive events', () => {
    //- arrange
    defineTestMessages();
    
    //* convert to anonymous serialised form as you would get from the web
    const command = new TestCommand_c100v1({c100_pointlessProp: '12345'}).convertToAnonymousObject();
    const testEvent1 = new TestEvent_e200v1({e200_results: [new TestEvent_e200v1.Results({e200_id: 1}), new TestEvent_e200v1.Results({e200_id: 2})]}).convertToAnonymousObject();
    let gotIt = false;
    
    mockEvent(command, [testEvent1]);
    
    //- listen for response to query
    const conversationId = commandHandler.handle(
        command,
        (event, postalEnvelope) => {

            expect(event instanceof getRegisteredMessageType("TestEvent_e200v1")).toBe(true);
            expect(event.e200_results[0] instanceof getRegisteredMessageType("TestEvent_e200v1.Results")).toBe(true);

            if (event.e200_results[0].e200_id === 1) {
                gotIt = true;
            }

        },
        0,
    );

    expect(postal.subscriptions[bus.channels.events][`#.${conversationId}`].length).toBe(1);
    bus.closeConversation(conversationId);

    expect(postal.subscriptions).toStrictEqual({});
    expect(gotIt).toBe(true);
    expect(typeof conversationId).toBe('string');
});

test('commands can receive events from cache', () => {
    //- arrange

    defineTestMessages();

    const command = new TestCommand_c100v1({c100_pointlessProp: '12345'}).convertToAnonymousObject();

    let gotIt = false;

    const testEvent1 = new TestEvent_e200v1({e200_results: [new TestEvent_e200v1.Results({e200_id: 1}), new TestEvent_e200v1.Results({e200_id: 2})]}).convertToAnonymousObject();
    cacheEvent(command, testEvent1);
    
    //- listen for response to query
    const conversationId = commandHandler.handle(
        command,
        (event, postalEnvelope) => {
            expect(event instanceof getRegisteredMessageType("TestEvent_e200v1")).toBe(true);
            expect(event.e200_results[0] instanceof getRegisteredMessageType("TestEvent_e200v1.Results")).toBe(true);

            if (event.e200_results[0].e200_id === 1) {
                gotIt = true;
            }
        },
        5,
    );

    expect(postal.subscriptions).toStrictEqual({});
    expect(gotIt).toBe(true);
    expect(typeof conversationId).toBe('undefined');
});

