import {useEffect} from 'react';
import postal from 'postal';
import bus from '../soap/bus';
import {optional, types, validateArgs} from "../soap/util";

export function useEvent(
    args,
    channel = bus.channels.events,
) {
    let { eventName, conversationId, onEventReceived } = args;
    
    useEffect(() => {
        let sub;
        if (!!args) {
            validateArgs(
                [{eventName}, types.string],
                [{conversationId}, types.string, optional],
                [{onEventReceived}, types.function]
            );
            sub = bus.subscribe(channel, eventName, onEventReceived, conversationId);
        }

        //* cleanup hook
        return () => {
            if (!!sub) postal.unsubscribe(sub);
        };
    }, [args]);
}
