import React from 'react';
import PropTypes from 'prop-types';
import ToastContainer from '../ToastContainer';
import { bus } from '../../modules/src/soap';
import { EVENTS } from '../../modules/src/classes/eventNames';
import { useSubscribeToApiEvent } from '../../modules/src/hooks/event';
import { displayToast, TOAST_TYPES } from '../ToastContainer/toast';
import * as S from './style';
import './font.css';

const withDefaultFont = component => (
  <div style={{ height: '100%', fontFamily: 'Muli' }}>{component}</div>
);

const AppWrapper = props => {
  useSubscribeToApiEvent(
    EVENTS.ApiErrorOccurred,
    apiResponse => displayToast(TOAST_TYPES.ERROR, apiResponse.message),
    bus.channels.errors,
  );

  const app = (
    <React.Fragment>
      {props.setHtmlAndBodyFullHeight && <S.HtmlBodyAndContentFullHeight />}
      <ToastContainer />
      {props.children}
    </React.Fragment>
  );

  return props.useDefaultFont ? withDefaultFont(app) : app;
};

AppWrapper.propTypes = {
  setHtmlAndBodyFullHeight: PropTypes.bool,
  useDefaultFont: PropTypes.bool,
};

AppWrapper.defaultProps = {
  setHtmlAndBodyFullHeight: true,
  useDefaultFont: false,
};

export default AppWrapper;
