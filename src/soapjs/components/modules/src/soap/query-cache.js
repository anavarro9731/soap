import _ from 'lodash';
import {md5Hash, types, validateArgs} from './util';


let cache = [];

export default {
    addOrReplace: (commandHash, event) => {
        validateArgs([{commandHash}, types.string], [{event}, types.object]);

        /* concurrency risk, prob not possible with 
            default browser STA, unless interrupted by async etc */
        _.remove(cache, i => i.commandHash === commandHash);
        cache.push({
            commandHash,
            event,
            timestamp: new Date().getTime(),
        });
    },
    
    clear: () => {cache = [];},

    find: (command, acceptableStalenessFactorInSeconds) => {
        validateArgs(
            [{command}, types.object],
            [{acceptableStalenessFactorInSeconds}, types.number],
        );

        const { headers, ...payload } = command;
        const commandHash = md5Hash(payload);

        let result;
        if (acceptableStalenessFactorInSeconds > 0) {
            console.log(cache);
            result = _.find(
                cache,
                i =>
                    i.commandHash === commandHash &&
                    new Date().getTime() - i.timestamp <=
                    acceptableStalenessFactorInSeconds * 1000,
            );
            if (result !== undefined) result = result.event;
        }
        console.log(result);
        return result;
    },
};
