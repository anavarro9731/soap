import React from 'react';
import ReactDOM from 'react-dom';
import { HashRouter as Router, Route } from 'react-router-dom';
import Link from './Link';

ReactDOM.render(
  <React.Fragment>
    <Router>
      <Route path="/" exact component={() => <div>Test Page</div>} />
      <Link to="/" textColour="red" hoverTextColour="blue">
        Test Link
      </Link>
    </Router>
  </React.Fragment>,
  document.getElementById('content'),
);
