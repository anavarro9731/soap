import React from 'react';
import ReactDOM from 'react-dom';
import MultiLineSelect from './MultiLineSelect';
import AppWrapper from '../AppWrapper';

const options = [
  { value: 'value 1', label: 'Label 1' },
  { value: 'value 2', label: 'Label 2' },
  { value: 'value 3', label: 'Label 3' },
  { value: 'value 4', label: 'Label 4' },
  { value: 'value 5', label: 'Label 5' },
  { value: 'value 6', label: 'Label 6' },
  { value: 'value 7', label: 'Label 7' },
  { value: 'value 8', label: 'Label 8' },
  { value: 'value 9', label: 'Label 9' },
  { value: 'value 10', label: 'Label 10' },
];

ReactDOM.render(
  <AppWrapper useDefaultFont>
    <MultiLineSelect options={options} />
  </AppWrapper>,
  document.getElementById('content'),
);
