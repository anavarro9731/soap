import {useEffect} from 'react';
import postal from 'postal';
import {bus} from '../soap';

export function useEvent(
    eventName,
    onEventReceived,
    channel = bus.channels.events,
    conversationId
) {
    useEffect(() => {
        const sub = bus.subscribe(channel, eventName, onEventReceived, conversationId);

        //* cleanup hook
        return () => {
            postal.unsubscribe(sub);
        };
    }, []);
}
