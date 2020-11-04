import { useEffect } from 'react';
import postal from 'postal';
import { bus } from '../soap';

export const useSubscribeToApiEvent = (
  eventName,
  onEventReceived,
  channel = bus.channels.events,
) => {
  useEffect(() => {
    const sub = bus.subscribe(channel, eventName, onEventReceived);
    //* cleanup hook
    return () => {
      postal.unsubscribe(sub);
    };
  }, []);
};
