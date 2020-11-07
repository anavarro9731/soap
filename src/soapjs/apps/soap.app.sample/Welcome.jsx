import React from 'react';
import WelcomeImage from './assets/images/hello_world.png';
import { AppWrapper } from '@soap/react';
import { i18n } from '@soap/modules';

const { translate, keys } = i18n;

const Welcome = () => (
  <AppWrapper>
      <img src={WelcomeImage} alt="Logo" />
      <h1>{translate(keys.back)}</h1>
  </AppWrapper>
);

export default Welcome;
