import React from 'react';
import WelcomeImage from 'url:./assets/images/hello_world.png';
import { i18n } from '@soap/modules';

const { translate, keys } = i18n;

const Welcome = () => (
  <div>
      <img src={WelcomeImage} alt="Logo" />
      <h1>{translate(keys.back)}</h1>
  </div>
);

export default Welcome;
