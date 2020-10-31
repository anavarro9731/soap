import React from 'react';
import PropTypes from 'prop-types';
import {
  defaultLightTextColour,
  defaultPrimaryBackgroundColour,
  defaultDisabledColour,
} from '../../modules/style/defaults';
import * as S from './style';

const Button = props => {
  return (
    <S.Button
      colour={props.textColour}
      backgroundColour={props.backgroundColour}
      verticalPadding={props.verticalPadding}
      horizontalPadding={props.horizontalPadding}
      onClick={props.onClick}
      border={props.border}
      noLeftBorder={props.noLeftBorder}
      borderRadiusLeft={props.borderRadiusLeft}
      borderRadiusRight={props.borderRadiusRight}
      width={props.width}
      height={props.height}
      hoverBackgroundColour={props.hoverBackgroundColour}
      disabled={props.disabled}
      disabledBackgroundColour={props.disabledBackgroundColour}
      type={props.type}
      fontSize={props.fontSize}
    >
      {props.children}
    </S.Button>
  );
};

Button.propTypes = {
  children: PropTypes.string,
  textColour: PropTypes.string,
  backgroundColour: PropTypes.string,
  onClick: PropTypes.func,
  verticalPadding: PropTypes.string,
  horizontalPadding: PropTypes.string,
  border: PropTypes.string,
  borderRadiusLeft: PropTypes.string,
  borderRadiusRight: PropTypes.string,
  hoverBackgroundColour: PropTypes.string,
  type: PropTypes.string,
  disabled: PropTypes.bool,
  disabledBackgroundColour: PropTypes.string,
  noLeftBorder: PropTypes.bool,
  fontSize: PropTypes.string,
};

Button.defaultProps = {
  textColour: defaultLightTextColour,
  backgroundColour: defaultPrimaryBackgroundColour,
  verticalPadding: '15px',
  horizontalPadding: '25px',
  border: 'none',
  borderRadiusLeft: '5px',
  borderRadiusRight: '5px',
  type: 'button',
  width: 'auto',
  height: 'auto',
  disabled: false,
  disabledBackgroundColour: defaultDisabledColour,
  noLeftBorder: false,
  fontSize: '16px',
};

export default Button;
