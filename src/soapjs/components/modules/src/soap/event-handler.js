import {bus, queryCache} from './index';
import {createRegisteredTypedMessageInstanceFromAnonymousObject} from './messages';

export default {

    handle: event => {

        const typedEventWrappedInProxy = createRegisteredTypedMessageInstanceFromAnonymousObject(event);

        const {headers} = typedEventWrappedInProxy;

        cacheSoTheSameQueriesAreNotRepeated(typedEventWrappedInProxy);

        bus.publish(
            headers.channel,
            headers.schema,
            typedEventWrappedInProxy,
            headers.conversationId,
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
