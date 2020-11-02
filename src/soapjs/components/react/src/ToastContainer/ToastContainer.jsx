import React from 'react';
import PropTypes from 'prop-types';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import {
  defaultTextColour,
  defaultHighlightColour,
  defaultLightBackgroundColour,
  defaultBorderColour,
} from '../style/defaults';
import * as S from './style';

const ToastContainer = props => <S.ToastContainer {...props} />;

ToastContainer.propTypes = {
  position: PropTypes.string,
  hideProgressBar: PropTypes.bool,
  autoCloseTimeout: PropTypes.oneOfType([PropTypes.bool, PropTypes.number]),
  width: PropTypes.string,
  progressBarColour: PropTypes.string,
  errorBackgroundColour: PropTypes.string,
  errorTextColour: PropTypes.string,
  errorTopBorderColour: PropTypes.string,
  warningBackgroundColour: PropTypes.string,
  warningTextColour: PropTypes.string,
  warningTopBorderColour: PropTypes.string,
  successBackgroundColour: PropTypes.string,
  successTextColour: PropTypes.string,
  successTopBorderColour: PropTypes.string,
};

ToastContainer.defaultProps = {
  position: toast.POSITION.TOP_RIGHT,
  hideProgressBar: false,
  autoCloseTimeout: false,
  width: '40%',
  progressBarColour: defaultBorderColour,
  errorBackgroundColour: '#f2dede',
  errorTextColour: '#a94442',
  errorTopBorderColour: '#a94442',
  warningBackgroundColour: defaultLightBackgroundColour,
  warningTextColour: defaultTextColour,
  warningTopBorderColour: defaultHighlightColour,
  successBackgroundColour: defaultLightBackgroundColour,
  successTextColour: defaultTextColour,
  successTopBorderColour: defaultHighlightColour,
};

export default ToastContainer;
