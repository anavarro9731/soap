import { useState, useEffect } from 'react';
import { commandHandler, bus } from '../soap';

export const useQuery = (
  query,
  {
    condition = true,
    refetchOnChange = [],
    acceptableStalenessFactorInSeconds = 0,
  } = {},
) => {
  const [queryResult, setQueryResult] = useState();

  const refetchOnChangeArray = Array.isArray(refetchOnChange)
    ? refetchOnChange
    : [refetchOnChange];

  const onResponse = data => {
    setQueryResult(data);
  };

  useEffect(() => {
    let conversationId = undefined;

    if (condition === true) {
      conversationId = commandHandler.handle(
        query,
        onResponse,
        acceptableStalenessFactorInSeconds,
      );
    }

    return () => {
      if (conversationId) {
        bus.closeConversation(conversationId);
      }
    };
  }, refetchOnChangeArray);

  return queryResult;
};

export const command = command => {
  const conversationId = commandHandler.handle(command, () => null, 0);
  bus.closeConversation(conversationId);
};
