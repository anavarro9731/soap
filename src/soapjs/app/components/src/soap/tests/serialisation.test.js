import messageDefinitions, {TestCommand_c101v1} from './test-messages'; 
import {createRegisteredTypedMessageInstanceFromAnonymousObject, toTypeName, registerMessageTypes } from "../messages";

test("test expanding command name", () => {
    registerMessageTypes(messageDefinitions);
   const typeName = toTypeName("TestCommand_c100v1");
   expect(typeName).toBe('Soap.Api.Sample.Messages.Commands.TestCommand_c100v1, Soap.Messages');
});

test("test expanding event name", () => {
    registerMessageTypes(messageDefinitions);
    const typeName = toTypeName("TestEvent_e200v1");
    expect(typeName).toBe('Soap.Api.Sample.Messages.Events.TestEvent_e200v1, Soap.Api.Sample.Messages');
});

test('test deserialisation of data type min values', () => {
    
    registerMessageTypes(messageDefinitions);
    const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("min")));
    createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    
});


test('test deserialisation of data type max values', () => {

    registerMessageTypes(messageDefinitions);
    const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("max")));
    createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);

});


test('test deserialisation of data type optional values', () => {

        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("optionalAreOptional")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
});

test('test deserialisation of data type required values', () => {

    
    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("requiredIsUndefined")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(7); //* all non-optional except for object should fail, object is always allowed to be null
    }

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("requiredIsNull")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(7); //* all non-optional except for object should fail, object is always allowed to be null
    }

});

test('test deserialisation of data type list', () => {

    registerMessageTypes(messageDefinitions);
    const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("emptylist")));
    createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    
    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("nullinlist")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); 
    }

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("undefinedinlist")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); 
    }

});

test('test deserialisation of invalid data in child object', () => {
    
    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("childcontainsnull")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(2); 
    }

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("childcontainsundefined")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
        throw "should have thrown";
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); 
    }

});

