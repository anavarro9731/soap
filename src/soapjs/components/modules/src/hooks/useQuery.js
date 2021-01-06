import {useEffect, useState} from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {toTypeName} from "../soap/messages";
import config from "../soap/config";

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {
    
        const [queryResult, setQueryResult] = useState();
        const [typesLoaded, setTypesLoaded] = useState(false);
        const [queuedMessage, setQueuedMessage] = useState(false);
                
        const onResponse = data => {
            setQueryResult(data);
        };
        
        useEffect(() => {
            /* right now this is only relevant for autoform C109 command which is not entered by a developer
            the developer must use the long name for now, or we have to change useEvent, and useCommand as well 
            and that will also slow things down */
            const typeName = toTypeName(query.$type);
            if (typeName === "types-not-loaded") {
                if (!queuedMessage) {
                    console.warn("queueing form til types loaded");
                    config.startupCallbacks.push(() => {
                        console.warn("enabling form");
                        setTypesLoaded(true);
                    });
                    setQueuedMessage(true);
                }
            } else {
                setTypesLoaded(true);
            }
            
            let conversationId = undefined;
            
            if (sendQuery && typesLoaded) {  
                query.$type = typeName; //convert from class short name to assembly qualified short name
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
        }, [sendQuery, typesLoaded]);

        return queryResult;
}
