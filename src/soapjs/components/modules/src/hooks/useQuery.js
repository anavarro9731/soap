import {useEffect, useState } from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {toTypeName} from "../soap/messages";
import {useIsConfigLoaded} from "./systemStateHooks";
import {useAuth} from "./useLogin";
import config from "../soap/config";

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {

    const [queryResult, setQueryResult] = useState();
    const configLoaded = useIsConfigLoaded("useQuery.js");
    
    const { 
        authReady
    } = useAuth();
    
    const onResponse = data => {
        setQueryResult(data);
    };
    
    useEffect(() => {

        let conversationId = undefined;
        if (config.debugSystemState) console.warn("status at useQuery", configLoaded, authReady, sendQuery);
        if (configLoaded && authReady && sendQuery) {
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

        /* returning a function is how you set a destructor on the hook's resources
        it is called *every* time the hook is run. */
        return () => {
            if (conversationId) {
                bus.closeConversation(conversationId);
            }
            if (config.debugSystemState) console.warn("useQuery destructor ran because one of the items in [sendQuery, configLoaded, authReady] changed or the component was mounted/unmounted, and any outstanding conversation was terminated.");
        };
        
    }, [sendQuery, configLoaded, authReady]);

    return queryResult;
}
