import messageDefinitions, {TestCommand_c101v1} from './test-messages'; 
import {createRegisteredTypedMessageInstanceFromAnonymousObject, getRegisteredMessageType, registerMessageTypes } from "../messages";


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

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("optional")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(10); //* 1/2 should pass
    }

});

test('test deserialisation of data type required values', () => {

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("required")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(10); //* 1/2 should pass
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
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); //* 1/2 should pass
    }

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("undefined")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); //* 1/2 should pass
    }

});

test('test deserialisation of invalid data in child object', () => {
    
    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("childcontainsnull")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(2); //* 1/2 should pass
    }

    try {
        registerMessageTypes(messageDefinitions);
        const anonymousCommand = JSON.parse(JSON.stringify(new TestCommand_c101v1("childcontainsundefined")));
        createRegisteredTypedMessageInstanceFromAnonymousObject(anonymousCommand);
    } catch(err) {
        const errorCount = (err.match(/\^/g)||[]).length;
        expect(errorCount).toBe(1); //* 1/2 should pass
    }

});

