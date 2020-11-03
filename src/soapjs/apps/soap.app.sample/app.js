import React from 'react';
import ReactDOM from 'react-dom';
import Welcome from './Welcome.jsx';
import 'sanitize.css/sanitize.css';

console.debug(Welcome);
ReactDOM.render(<Welcome />, document.getElementById('content'));
