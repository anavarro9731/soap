import React from 'react';
import ReactDOM from 'react-dom';
import LoadingOverlay from './LoadingOverlay';
import { useQuery } from '../../modules/src/hooks/api-request';
import { mockEvent } from '../../modules/src/soap';
import Accordion from '../Accordion';
import AppWrapper from '../AppWrapper';

const testQuery = {
  schema: 'testQuery',
  pointlessprop: '2342342342',
};

const QueryCall = () => {
  mockEvent(testQuery, { testResponse: 'testResponse' });

  useQuery(testQuery);
  return <div></div>;
};

ReactDOM.render(
  <AppWrapper>
    <LoadingOverlay requests={[testQuery]}>
      <QueryCall />
      <Accordion
        sections={[
          {
            id: '1',
            title: 'Fanta',
            rightAlignedContent: 'X',
            content: 'Failed because of a reason',
          },
          {
            id: '2',
            title: 'Chicken nuggets',
            rightAlignedContent: '✔',
          },
        ]}
      />
    </LoadingOverlay>
  </AppWrapper>,
  document.getElementById('content'),
);
