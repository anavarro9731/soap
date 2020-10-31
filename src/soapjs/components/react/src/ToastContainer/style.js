import styled from 'styled-components';
import { ToastContainer as ToastifyContainer } from 'react-toastify';

export const ToastContainer = styled(ToastifyContainer).attrs(props => ({
  position: props.position,
  hideProgressBar: props.hideProgressBar,
  autoClose: props.autoCloseTimeout,
}))`
  width: ${props => props.width};

  button[aria-label='close'] {
    color: #000;
  }

  .Toastify__toast-container {
  }

  .Toastify__toast {
  }

  .Toastify__toast--error {
    background-color: ${props => props.errorBackgroundColour};
    color: ${props => props.errorTextColour};
    border-top: 7px solid ${props => props.errorTopBorderColour};
  }
  .Toastify__toast--warning {
    background-color: ${props => props.warningBackgroundColour};
    color: ${props => props.warningTextColour};
    border-top: 7px solid ${props => props.warningTopBorderColour};
  }
  .Toastify__toast--success {
    background-color: ${props => props.successBackgroundColour};
    color: ${props => props.successTextColour};
    border-top: 7px solid ${props => props.successTopBorderColour};
  }
  .Toastify__toast-body {
    margin: 10px;
  }
  .Toastify__progress-bar {
    background-color: ${props => props.progressBarColour};
  }
`;
