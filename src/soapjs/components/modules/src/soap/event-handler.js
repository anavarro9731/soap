import bus from './bus';
import queryCache from './query-cache';
import {createRegisteredTypedMessageInstanceFromAnonymousObject, headerKeys} from './messages';
import {getHeader} from './util';

export default {

    handle: event => {

        //* expects headers to be set properly in API
        
        const schema = getHeader(event, headerKeys.schema);
        
        if (schema === "Soap.Interfaces.Messages.E001v1_MessageFailed") {
            
            bus.publish(
                bus.channels.events,
                getHeader(event, headerKeys.schema),
                event,
                getHeader(event, headerKeys.commandConversationId)
            );
            
        } else {
            
            const typedEventWrappedInProxy = createRegisteredTypedMessageInstanceFromAnonymousObject(event);

            cacheSoTheSameQueriesAreNotRepeated(typedEventWrappedInProxy);

            bus.publish(
                bus.channels.events,
                getHeader(event, headerKeys.schema),
                typedEventWrappedInProxy,
                getHeader(event, headerKeys.commandConversationId)
            );


            function cacheSoTheSameQueriesAreNotRepeated() {
                const commandHash = getHeader(event, headerKeys.commandHash);
                if (!!commandHash) { //* not all events derive from commands so many will not have a commandHash
                    queryCache.addOrReplace(
                        commandHash,
                        typedEventWrappedInProxy,
                    );
                } else {
                    //* then nothing will be cached so you will ask again every time (e.g. testing) 
                }
            }

        }

    }
};
