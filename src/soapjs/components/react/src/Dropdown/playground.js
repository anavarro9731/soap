import React from 'react';
import ReactDOM from 'react-dom';
import Dropdown from './Dropdown';

const options = [
  { value: 'chocolate', label: 'Chocolate' },
  { value: 'vanilla', label: 'Vanilla' },
];

ReactDOM.render(
  <Dropdown
    options={options}
    value="chocolate"
    width="500px"
    onChange={value => console.log(value)}
  />,
  document.getElementById('content'),
);
