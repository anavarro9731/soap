import _ from 'lodash';
import { types, optional, validateArgs, md5Hash } from './util.js';
import { ApiQuery, ApiEvent } from './messages.js';

let cache = [];

export default {
  addOrReplace: (queryHash, event) => {
    validateArgs([{ queryHash }, types.string], [{ event }, ApiEvent]);

    /* concurrency risk, prob not possible with 
        default browser STA, unless interrupted by async etc */
    _.remove(cache, i => i.queryHash === queryHash);
    cache.push({
      queryHash,
      event,
      timestamp: new Date().getTime(),
    });
  },

  query: (query, acceptableStalenessFactorInSeconds) => {
    validateArgs(
      [{ query }, ApiQuery],
      [{ acceptableStalenessFactorInSeconds }, types.number],
    );

    const queryHash = md5Hash(query);

    let result;
    if (acceptableStalenessFactorInSeconds > 0) {
      result = _.find(
        cache,
        i =>
          i.queryHash === queryHash &&
          new Date().getTime() - i.timestamp <=
            acceptableStalenessFactorInSeconds * 1000,
      ).event;
    }
    return result;
  },
};
