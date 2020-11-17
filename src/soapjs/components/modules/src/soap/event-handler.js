import {bus, queryCache} from './index';
import {createRegisteredTypedMessageInstanceFromAnonymousObject} from './messages';

export default {

    handle: event => {

        const typedEventWrappedInProxy = createRegisteredTypedMessageInstanceFromAnonymousObject(event, event.headers.schema);

        const {headers} = typedEventWrappedInProxy;

        cacheSoTheSameQueriesAreNotRepeated(typedEventWrappedInProxy);

        bus.publish(
            bus.channels.events,
            headers.schema,
            typedEventWrappedInProxy,
            headers.commandConversationId,
        );

        function cacheSoTheSameQueriesAreNotRepeated() {
            if (!!headers.commandHash) { //* not all events derive from commands so many will not have a commandHash
                queryCache.addOrReplace(
                    headers.commandHash,
                    typedEventWrappedInProxy,
                );
            } else {
                //* then nothing will be cached so you will ask again every time (e.g. testing) 
            }
        }


    }
};
