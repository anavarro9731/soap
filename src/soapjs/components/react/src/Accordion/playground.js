import React from 'react';
import ReactDOM from 'react-dom';
import Accordion from './Accordion';

ReactDOM.render(
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
      {
        id: '3',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
      {
        id: '4',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
      {
        id: '5',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
      {
        id: '6',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
      {
        id: '7',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
      {
        id: '8',
        title: 'Fanta',
        rightAlignedContent: 'X',
        content: 'Failed because of a reason',
      },
    ]}
  />,
  document.getElementById('content'),
);
