import * as __ from './util.js';
import { bus, queryCache } from './index.js';
import {
  validateArgs,
  types,
  recursivelyConvertObjectNullPropertiesToUndefined,
} from './util.js';
import config from './config';

export default {
  handle: eventEnvelope => {
    validateArgs([{ eventEnvelope }, types.object]);

    if (eventEnvelope.headers.channel === bus.channels.queries) {
      if (!!eventEnvelope.headers.queryHash)
        //- then nothing will be cached so you will ask again every time (e.g. testing)
        queryCache.addOrReplace(
          eventEnvelope.headers.queryHash,
          eventEnvelope.payload,
        );
    }

    let converted = undefined;
    try {
      converted = wrapInProxy(
        parse(
          JSON.stringify(
            recursivelyConvertObjectNullPropertiesToUndefined(
              eventEnvelope.payload,
            ),
          ),
        ),
      );
      
    } catch (error) {
      console.error('!!CONVERSION ERROR!! ', error);
    }

    bus.publish(
      eventEnvelope.headers.channel,
      eventEnvelope.headers.schema,
      converted,
      eventEnvelope.headers.conversationId,
    );

    function wrapInProxy(data) {
      return new Proxy(data, {
        set: function(target, property, value) {
          // First give the target a chance to handle it
          if (Object.keys(target).indexOf(property) !== -1) {
            return target[property];
          }
          console.error(
            `MISSING PROPERTY EXCEPTION: Attempted to write to ${typeof target}.${property.toString()} but it does not exist`,
          );
        },
        get: function(target, property) {
          // First give the target a chance to handle it
          if (
            Object.keys(target).indexOf(property) !== -1 &&
            property !== 'toJSON'
          ) {
            return target[property];
          } else if (property !== 'toJSON')
            console.error(
              `MISSING PROPERTY EXCEPTION: Attempted to read from ${typeof target}.${property.toString()} but it does not exist`,
            );
        },
      });
    }

    function parse(data) {
      return JSON.parse(data, (key, value) => {
        if (isObjectLike(value) && value.hasOwnProperty('$type')) {
          if (value instanceof Array) {
            return value.map(function(element, index) {
              return make(element);
            });
          }
          return make(value);
        }
        return value;
      });

      function isObjectLike(value) {
        return typeof value === 'object' && value !== null;
      }

      function make(payloadPortion) {
        const { $type, ...values } = payloadPortion;
        const classTypeRegexMatches = $type.match(/\.([^.]+),/i);
        const classType = classTypeRegexMatches[1].replace(/\+/g, '.');
        const newFunction = new Function(
          'messages',
          'values',
          `return new messages.${classType}(values)`,
        );
        return newFunction(config.getMessages(), values);
      }
    }
  },
};
