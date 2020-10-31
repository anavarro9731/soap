import React from 'react';
import ReactDOM from 'react-dom';
import { HashRouter as Router, Route } from 'react-router-dom';
import DropdownButton from './DropdownButton';

const options = [
  {
    onClick: () => console.log('first option clicked'),
    label: 'Edit Page',
  },
  {
    route: '/test',
    label: 'Add Page',
  },
];

const Dropdown = () => (
  <DropdownButton
    options={options}
    buttonText="Click here"
    width="500px"
    position={DropdownButton.POSITIONS.LEFT}
  />
);

ReactDOM.render(
  <Router>
    <Route path="/" exact component={Dropdown} />
  </Router>,
  document.getElementById('content'),
);
