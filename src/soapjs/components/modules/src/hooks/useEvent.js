import {useEffect} from 'react';
import postal from 'postal';
import bus from '../soap/bus';

export function useEvent(
    args,
    channel = bus.channels.events,
) {
    const { eventName, conversationId, onEventReceived } = args;
    
    useEffect(() => {

        let sub;
        if (!!args) {
            sub = bus.subscribe(channel, eventName, onEventReceived, conversationId);
        }

        //* cleanup hook
        return () => {
            if (!!sub) postal.unsubscribe(sub);
        };
    }, [args]);
}
