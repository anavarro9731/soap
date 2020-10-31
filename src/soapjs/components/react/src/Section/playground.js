import React from 'react';
import ReactDOM from 'react-dom';
import Section from './Section';

ReactDOM.render(
  <Section
    title="Test Title"
    rightAlignedContent="Right aligned content"
    startExpanded
  >
    aa
  </Section>,
  document.getElementById('content'),
);
