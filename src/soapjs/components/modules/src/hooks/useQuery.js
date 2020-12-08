import {useEffect, useState} from 'react';
import {bus, commandHandler, validateArgs} from '../soap';

// export class queryOptions {
//     constructor(condition, arrayWhichCausesRequeryOnChange, acceptableStalenessFactorInSeconds) {
//         this.condition = condition ?? true;
//         this.arrayWhichCausesRequeryOnChange = (Array.isArray(arrayWhichCausesRequeryOnChange)
//             ? arrayWhichCausesRequeryOnChange
//             : [arrayWhichCausesRequeryOnChange]) ?? [];
//         this.acceptableStalenessFactorInSeconds = acceptableStalenessFactorInSeconds ?? 0;
//     }
// }

export function useQuery(query, acceptableStalenessFactorInSeconds = 0) {
    
        const [queryResult, setQueryResult] = useState();

        const onResponse = data => {
            setQueryResult(data);
        };

        useEffect(() => {
            let conversationId = undefined;

            if (!!query) {
                conversationId = commandHandler.handle(
                    query,
                    onResponse,
                    acceptableStalenessFactorInSeconds,
                );
            }

            //* returning a function is how you set a destructor on the hook's resources 
            return () => {
                if (conversationId) {
                    bus.closeConversation(conversationId);
                }
            };
        }, [query]);

        return queryResult;
}
