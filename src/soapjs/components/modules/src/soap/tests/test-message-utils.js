import * as testMessages from './test-messages';
import * as messages from '../messages';
import { registerMessageTypes } from '../messages';


let messageSchemaTemplates = [];

export function defineTestMessages() {
    for (const moduleKey in testMessages) {
        const msgConstructor = testMessages[moduleKey];
        if (messages.ApiMessage.isPrototypeOf(msgConstructor)) {
            console.log(msgConstructor.toString());
            const anonymousVersion = msgConstructor.CreateTemplate();
            messageSchemaTemplates.push(anonymousVersion);
        }
    }
    registerMessageTypes(messageSchemaTemplates);
    
}   