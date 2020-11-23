import {mockEvent, default as commandHandler, cacheEvent} from '../command-handler';
import messageDefinitions from './test-messages';
import bus from '../bus';
import postal from 'postal';
import {createRegisteredTypedMessageInstanceFromAnonymousObject, getRegisteredMessageType, registerMessageTypes } from "../messages";

test('event schema created without error', () => {
    registerMessageTypes(messageDefinitions);
    const anonymousEvent = {e200_results: [{e200_id: 10}, {e200_id: 20}], $type:'Soap.Api.Sample.Messages.Events.TestEvent_e200v1, Soap.Api.Sample.Messages', headers : []};
    const testEvent_e200v1 = createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousEvent);
});

test('commands can receive receive events', () => {
    //* arrange
    registerMessageTypes(messageDefinitions);
    
    //* create from registered message as you would in real code
    const testCommand_c100v1 = createRegisteredTypedMessageInstanceFromAnonymousObject({c100_pointlessProp: '12345', $type:'Soap.Api.Sample.Messages.Commands.TestCommand_c100v1, Soap.Api.Sample.Messages', headers: []});
    //* take as object literal as you would in real code
    const testEvent_e200v1 = {e200_results: [{e200_id: 10}, {e200_id: 20}], $type:'Soap.Api.Sample.Messages.Events.TestEvent_e200v1, Soap.Api.Sample.Messages', headers : []};
    let gotIt = false;
    
    mockEvent(testCommand_c100v1, [testEvent_e200v1]);
    
    //* listen for response to query
    const conversationId = commandHandler.handle(
        testCommand_c100v1,
        (event, postalEnvelope) => {

            expect(event instanceof getRegisteredMessageType("Soap.Api.Sample.Messages.Events.TestEvent_e200v1")).toBe(true);
            expect(event.e200_results[0] instanceof getRegisteredMessageType("Soap.Api.Sample.Messages.Events.TestEvent_e200v1.Results")).toBe(true);

            if (event.e200_results[0].e200_id === 10) {
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
    //* arrange

    registerMessageTypes(messageDefinitions);

    //* create from registered message as you would in real code
    const testCommand_c100v1 = createRegisteredTypedMessageInstanceFromAnonymousObject({c100_pointlessProp: '12345', $type:'Soap.Api.Sample.Messages.Commands.TestCommand_c100v1, Soap.Api.Sample.Messages', headers: []});
    //* take as object literal as you would in real code
    const testEvent_e200v1 = {e200_results: [{e200_id: 10}, {e200_id: 20}], $type:'Soap.Api.Sample.Messages.Events.TestEvent_e200v1, Soap.Api.Sample.Messages', headers : []};
    let gotIt = false;
    
    cacheEvent(testCommand_c100v1, testEvent_e200v1);

    //* listen for response to query
    const conversationId = commandHandler.handle(
        testCommand_c100v1,
        (event, postalEnvelope) => {
            expect(event instanceof getRegisteredMessageType("Soap.Api.Sample.Messages.Events.TestEvent_e200v1")).toBe(true);
            expect(event.e200_results[0] instanceof getRegisteredMessageType("Soap.Api.Sample.Messages.Events.TestEvent_e200v1.Results")).toBe(true);

            if (event.e200_results[0].e200_id === 10) {
                gotIt = true;
            }
        },
        5,
    );

    expect(postal.subscriptions).toStrictEqual({});
    expect(gotIt).toBe(true);
    expect(typeof conversationId).toBe('undefined');
});

