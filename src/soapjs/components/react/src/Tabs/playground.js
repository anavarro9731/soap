import React from 'react';
import ReactDOM from 'react-dom';
import Tabs from './Tabs';
import AppWrapper from '../AppWrapper';

const commonStylingProps = { hoverAndActiveTabBackgroundColour: '#3b3f4a' };

ReactDOM.render(
  <AppWrapper>
    <div style={{ padding: '10px', height: '90%', width: '74%' }}>
      <Tabs>
        <Tabs.Tab title="Product and Something" {...commonStylingProps}>
          <div>some text</div>
        </Tabs.Tab>
        <Tabs.Tab initiallySelected title="Hardware" {...commonStylingProps}>
          <div>
            <input type="text" />
          </div>
        </Tabs.Tab>
        <Tabs.Tab title="Information" {...commonStylingProps}>
          <div></div>
        </Tabs.Tab>
      </Tabs>
    </div>
  </AppWrapper>,
  document.getElementById('content'),
);
