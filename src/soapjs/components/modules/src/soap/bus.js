import { types, optional, validateArgs } from './util';
import postal from 'postal';
import config from './config';

export default {
  publish: function(channel, schema, data, conversationId) {
    validateArgs(
      [{ channel }, types.string],
      [{ schema }, types.string],
      [{ data }, types.object],
      [{ conversationId }, types.string, optional],
    );

    let topic = schema;
    topic += `.${conversationId ?? "00000000-0000-0000-0000-000000000000"}`;

    config.logger.log(
      `PUBLISHING ${JSON.stringify(data,null,2)} to channel: ${channel} topic: ${topic}`,
    );

    postal.publish({
      channel: channel,
      topic: topic,
      data: data,
    });
  },

  subscribe: function(channel, schema, callback, conversationId) {
    validateArgs(
      [{ channel }, types.string],
      [{ schema }, types.string],
      [{ callback }, types.function],
      [{ conversationId }, types.string, optional],
    );

    let topic = schema;
    topic += `.${conversationId ?? "#"}`;

    const sub = postal.subscribe({
      channel: channel,
      topic: topic,
      callback: callback,
    });

    config.logger.log(`SUBSCRIBED to channel: ${channel} topic: ${topic}`);

    return sub;
  },

  closeConversation: function(conversationId) {
    validateArgs([{ conversationId }, types.string]);

    postal.unsubscribeFor(s => s.topic === `#.${conversationId}`);

    config.logger.log(
      `UNSUBSCRIBED to all messages in conversation: ${conversationId}`,
    );
  },

  channels: {
    events: 'events',
    commands: 'commands',
    errors: 'errors',
  },
};
