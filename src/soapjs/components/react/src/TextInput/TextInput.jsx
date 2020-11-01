import React from 'react';
import PropTypes from 'prop-types';
import { defaultBorderColour } from '../../modules/src/style/defaults';
import * as S from './style';

const TextInput = props => (
  <S.TextInput
    value={props.value}
    onChange={e => props.onChange(e.target.value)}
    onBlur={props.onBlur}
    type={props.type}
    disabled={props.disabled}
    autoComplete="off"
    // formik
    name={props.name}
    // styled-components
    width={props.width}
    border={props.border}
  />
);

TextInput.propTypes = {
  name: PropTypes.string,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  onChange: PropTypes.func,
  onBlur: PropTypes.func,
  width: PropTypes.string,
  disabled: PropTypes.bool,
  type: PropTypes.string,
  border: PropTypes.string,
};

TextInput.defaultProps = {
  width: '100%',
  disabled: false,
  type: 'text',
  border: `1px solid ${defaultBorderColour}`,
};

export default TextInput;
