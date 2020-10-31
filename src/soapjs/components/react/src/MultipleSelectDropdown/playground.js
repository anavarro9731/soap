import React, { useState } from 'react';
import ReactDOM from 'react-dom';
import MultipleSelectDropdown from './MultipleSelectDropdown';

const options = [
  { value: 'chocolate', label: 'Chocolate' },
  { value: 'vanilla', label: 'Vanilla' },
];

const MultipleSelectDropdownConsumer = () => {
  const [selectedOptions, setSelectedOptions] = useState([]);

  return (
    <MultipleSelectDropdown
      options={options}
      value={selectedOptions}
      width="500px"
      onChange={value => setSelectedOptions(value)}
    />
  );
};

ReactDOM.render(
  <MultipleSelectDropdownConsumer />,
  document.getElementById('content'),
);
