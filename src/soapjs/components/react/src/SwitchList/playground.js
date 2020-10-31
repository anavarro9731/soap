import React, { useRef } from 'react';
import ReactDOM from 'react-dom';
import SwitchList from './SwitchList';
import { getSelectRefOptions } from '../../modules/utils/refs';
import AppWrapper from '../AppWrapper';

const initialFirstListOptions = [
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

const initialSecondListOptions = [
  { value: 'value 11', label: 'Label 11' },
  { value: 'value 12', label: 'Label 12' },
  { value: 'value 13', label: 'Label 13' },
  { value: 'value 14', label: 'Label 14' },
  { value: 'value 15', label: 'Label 15' },
  { value: 'value 16', label: 'Label 16' },
  { value: 'value 17', label: 'Label 17' },
  { value: 'value 18', label: 'Label 18' },
  { value: 'value 19', label: 'Label 19' },
  { value: 'value 20', label: 'Label 20' },
  { value: 'value 21', label: 'Label 21' },
  { value: 'value 22', label: 'Label 22' },
];

const ComponentUsingSwitchList = () => {
  const firstListRef = useRef();
  const secondListRef = useRef();

  const handleClick = () => {
    console.log('First List: ', getSelectRefOptions(firstListRef));
    console.log('Second List: ', getSelectRefOptions(secondListRef));
  };

  return (
    <React.Fragment>
      <button style={{ marginBottom: '50px' }} onClick={handleClick}>
        Console.log list values
      </button>
      <SwitchList
        initialFirstListOptions={initialFirstListOptions}
        initialSecondListOptions={initialSecondListOptions}
        firstListRef={firstListRef}
        secondListRef={secondListRef}
        firstListTitle="First List"
        secondListTitle="Second List"
      />
    </React.Fragment>
  );
};

ReactDOM.render(
  <AppWrapper useDefaultFont>
    <ComponentUsingSwitchList />
  </AppWrapper>,
  document.getElementById('content'),
);
