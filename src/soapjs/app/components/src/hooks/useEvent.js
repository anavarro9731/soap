import {useEffect} from 'react';
import postal from 'postal';
import bus from '../soap/bus';
import {optional, types, validateArgs} from "../soap/util";
import config from "../soap/config";

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
            config.logger.log(`UNSUBSCRIBED to channel:${sub.channel}, topic:${sub.topic}`);
            if (!!sub) postal.unsubscribe(sub);
        };
    }, [args]);
}
