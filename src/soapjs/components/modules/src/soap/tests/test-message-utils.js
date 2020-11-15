import * as testMessages from './test-messages';
import { registerMessageTypes } from '../messages';


let messageSchemaTemplates = [];

export function defineTestMessages() {
    for (const moduleKey in testMessages) {
        const msgConstructor = testMessages[moduleKey];
        if (msgConstructor.CreateTemplate) {
            const anonymousVersion = msgConstructor.CreateTemplate();
            messageSchemaTemplates.push(anonymousVersion);
        }
    }
    registerMessageTypes(messageSchemaTemplates);
    
}   