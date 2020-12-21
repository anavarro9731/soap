import {useEffect, useState} from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {
    
        const [queryResult, setQueryResult] = useState();
        
        const onResponse = data => {
            setQueryResult(data);
        };

        useEffect(() => {
            let conversationId = undefined;

            if (!!sendQuery) {
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
        }, [sendQuery]);

        return queryResult;
}
