import {useEffect, useState, useCallback } from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";
import {useAuth} from "./useAuth";
import config from "../soap/config";
import {toTypeName} from "../soap/messages";

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {
        
    const [queryResult, setQueryResult] = useState();
    const configLoaded = useIsConfigLoaded("UseQuery");
    const { authReady } = useAuth("UseQuery");
    const [refreshIndex, setRefreshIndex] = useState(0);
    
    useEffect(() => {
        
        if (config.debugSystemState){
            const t = new Date().getTime().toString();
            console.warn(`Status at useQuery at ${t} `, configLoaded, authReady, sendQuery);  
        }

        let conversationId = undefined;
        const onResponse = data => setQueryResult(data);
        
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

        /* returning a function is how you set a destructor on the hook's resources it is called every time useEffect is run. */
        return function destructor() {
            
            if (conversationId) {
                bus.closeConversation(conversationId);
            }
            if (config.debugSystemState) console.warn(
                "useQuery destructor ran because one of the items in " +
                "[sendQuery, configLoaded, authReady, refreshIndex] changed or the component was " +
                "unmounted resulting in termination of any outstanding conversation.");
        };
        
        
    }, [sendQuery, configLoaded, authReady, refreshIndex]);
    
    
    const refresh = useCallback(() => {
        setRefreshIndex(index => index + 1);
        setQueryResult(null);
    }, []);
    
    return [queryResult, refresh];
}