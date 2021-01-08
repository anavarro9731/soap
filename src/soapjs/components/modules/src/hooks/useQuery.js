import {useEffect, useState} from 'react';
import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {toTypeName} from "../soap/messages";
import config from "../soap/config";
import {uuidv4} from "../soap/util";
import _ from 'lodash'

export function useQuery({query, sendQuery = true, acceptableStalenessFactorInSeconds = 0}) {

    const [queryResult, setQueryResult] = useState();
    const [typesLoaded, setTypesLoaded] = useState(false);
    const [queuedMessage, setQueuedMessage] = useState(false);
    
    const onResponse = data => {
        setQueryResult(data);
    };

    useEffect(() => {
        const typeName = toTypeName(query.$type);
        
        let queuedMessageId;
        
        if (typeName === "types-not-loaded") {
            if (!queuedMessage) {
                console.warn("queueing form until types loaded");
                queuedMessageId = uuidv4();
                config.startupCallbacks.push({
                    f: () => {
                        console.warn("dequeuing form");
                        setTypesLoaded(true);
                    }, id: queuedMessageId
                });
                setQueuedMessage(true);
            }
        } else {
            setTypesLoaded(true);
        }

        let conversationId = undefined;

        if (sendQuery && typesLoaded) {
            query.$type = typeName; //convert from class short name to assembly qualified short name
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
            if (queuedMessageId) {
                _.remove(config.startupCallbacks, i => i.id === queuedMessageId);
            }

        };
    }, [sendQuery, typesLoaded]);

    return queryResult;
}
