import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const Checkbox = props => {
  const checkboxId = `checkbox-component-${props.name || Math.random()}`;

  const label = (
    <S.Label
      htmlFor={checkboxId}
      disabled={props.disabled}
      labelOnLeftSide={props.labelOnLeftSide}
    >
      {props.checkboxLabel}
    </S.Label>
  );
  return (
    <S.CheckboxWithLabel>
      {props.labelOnLeftSide && label}
      <S.Checkbox
        id={checkboxId}
        checked={props.value}
        onChange={e => props.onChange(e.target.checked)}
        onBlur={props.onBlur}
        type="checkbox"
        disabled={props.disabled}
        // formik
        name={props.name}
      />
      {!props.labelOnLeftSide && label}
    </S.CheckboxWithLabel>
  );
};

Checkbox.propTypes = {
  name: PropTypes.string,
  value: PropTypes.bool,
  onChange: PropTypes.func,
  onBlur: PropTypes.func,
  disabled: PropTypes.bool,
  checkboxLabel: PropTypes.string,
  labelOnLeftSide: PropTypes.bool,
};

Checkbox.defaultProps = {
  disabled: false,
  value: false,
  labelOnLeftSide: false,
};

export default Checkbox;
