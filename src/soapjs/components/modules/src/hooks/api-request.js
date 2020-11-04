import {useEffect, useState} from 'react';
import {bus, commandHandler, types, validateArgs} from '../soap';

export function useQueryOptions(condition, arrayWhichCausesRequeryOnChange, acceptableStalenessFactorInSeconds) {
    this.condition = condition ?? true;
    this.arrayWhichCausesRequeryOnChange =  Array.isArray(arrayWhichCausesRequeryOnChange)
        ? arrayWhichCausesRequeryOnChange
        : [arrayWhichCausesRequeryOnChange];
    this.acceptableStalenessFactorInSeconds = acceptableStalenessFactorInSeconds ?? 0;
}

export const useQuery = function (
    query,
    options = new useQueryOptions()) {

    validateArgs([options, useQueryOptions]);
    
    const [queryResult, setQueryResult] = useState();

    const onResponse = data => {
        setQueryResult(data);
    };

    useEffect(() => {
        let conversationId = undefined;

        if (options.condition === true) {
            conversationId = commandHandler.handle(
                query,
                onResponse,
                options.acceptableStalenessFactorInSeconds,
            );
        }

        //* cleanup hook 
        return () => {
            if (conversationId) {
                bus.closeConversation(conversationId);
            }
        };
    }, options.arrayWhichCausesRequeryOnChange);

    return queryResult;
};

export const command = command => {
    const conversationId = commandHandler.handle(command, () => null, 0);
    bus.closeConversation(conversationId);
};
