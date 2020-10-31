import React from 'react';
import WelcomeImage from './assets/images/hello_world.png';
import AppWrapper from '@soapjs/components.app-wrapper';
import {
  translate,
  languages,
  useNewLanguage,
} from '@soapjs/components.i18n';
import config from './config';
import { appKeys, translations } from './modules/translations';

useNewLanguage(languages.EN_CUSTOM, translations);

const Welcome = () => (
  <AppWrapper>
      <img src={WelcomeImage} alt="Logo" />
      <h1>To Our App</h1>
  </AppWrapper>
);

export default Welcome;
