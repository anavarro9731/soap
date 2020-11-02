import React from 'react';
import WelcomeImage from './assets/images/hello_world.png';
import AppWrapper from '@soap/react';
import {
  translate,
  languages,
  useNewLanguage,
} from '@soap/modules/lib/i18n/index';
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
