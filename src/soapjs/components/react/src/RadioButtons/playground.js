import React, { useState } from 'react';
import ReactDOM from 'react-dom';
import RadioButtons from './RadioButtons';

const options = [
  { value: 'chocolate', label: 'Chocolate' },
  { value: 'vanilla', label: 'Vanilla' },
  { value: 'strawberry', label: 'Strawberry' },
];

const RadioButtonsConsumer = () => {
  const [selectedValue, setSelectedOptions] = useState();

  console.log('Selected Value:', selectedValue);

  return (
    <RadioButtons
      options={options}
      value={selectedValue}
      onChange={value => setSelectedOptions(value)}
    />
  );
};

ReactDOM.render(<RadioButtonsConsumer />, document.getElementById('content'));
