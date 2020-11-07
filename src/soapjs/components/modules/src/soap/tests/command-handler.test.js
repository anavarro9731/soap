import {mockEvent, default as commandHandler, cacheEvent} from '../command-handler';
import {TestEvent_e200v1, TestCommand_c100v1 } from './test-messages';
import bus from '../bus';
import postal from 'postal';
import {getRegisteredMessageType} from "../messages";


test('commands can receive receive events', () => {
    //- arrange
    const command = new TestCommand_c100v1({c100_pointlessProp: '12345'});

    let gotIt = false;

    mockEvent(command, [new TestEvent_e200v1({e200_results: [new TestEvent_e200v1.Results({e200_id: 1}), new TestEvent_e200v1.Results({e200_id: 2})]})]);
    
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
    const command = new TestCommand_c100v1({c100_pointlessProp: '12345'});

    let gotIt = false;
    
    const response = new TestEvent_e200v1({e200_results: [new TestEvent_e200v1.Results({e200_id: 1}), new TestEvent_e200v1.Results({e200_id: 2})]})
    cacheEvent(command, response);
    
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

