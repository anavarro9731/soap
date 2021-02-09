import {useEffect, useState, useReducer} from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {toTypeName} from "../soap/messages";
import config from "../soap/config";
import { useIsMounted } from './useIsMounted';

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {

    const [queryResult, setQueryResult] = useState();
    
    const onResponse = data => {
        setQueryResult(data);
    };

    const [, forceUpdate] = useReducer(x => x + 1, 0);
    const isMounted = useIsMounted();
    
    useEffect(() => {
        
        if (!config.isLoaded) {
            console.warn("queueing query until config state loaded");
            config.onLoaded(() => {
                //* could have been unmounted while config was loading, avoid console error if so
                console.warn("query callback ran 1");
                if (isMounted) {
                    console.warn("query callback ran 2");
                    forceUpdate();    
                }                 
            });
        }
        
        let conversationId = undefined;

        if (sendQuery && config.isLoaded) {
            query.$type = toTypeName(query.$type); //convert from class short name to assembly qualified short name
            if (!query.headers) {
                query.headers = [];
            }
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
