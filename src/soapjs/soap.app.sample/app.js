import React from 'react';
import 'sanitize.css/sanitize.css';
import ReactDOM from 'react-dom';
import FormControl from "./FormControl";

import { translate, addTranslations } from '@soap/modules';
import translations from "./translations/en-soap.app.sample-default";
import wordKeys from './translations/word-keys'

addTranslations(translations);

ReactDOM.render(<FormControl formEventName="E500v1_GetC107Form"/>, document.getElementById('content'));
