import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const RadioButtons = props => {
  const handleChange = event => props.onChange(event.target.value);

  return (
    <S.Grid radioButtonsPerRow={props.radioButtonsPerRow}>
      {props.options.map(option => (
        <S.MarginTopAndButton key={option.value}>
          <S.RadioButton
            type="radio"
            id={option.value}
            value={option.value}
            checked={option.value === props.value}
            onChange={handleChange}
            name={props.name}
            onBlur={props.onBlur}
            disabled={props.disabled}
          />
          <S.Label htmlFor={option.value}>{option.label}</S.Label>
        </S.MarginTopAndButton>
      ))}
    </S.Grid>
  );
};

RadioButtons.propTypes = {
  radioButtonsPerRow: PropTypes.number,
  name: PropTypes.string,
  value: PropTypes.string,
  onBlur: PropTypes.func,
  onChange: PropTypes.func,
  options: PropTypes.arrayOf(
    PropTypes.shape({
      value: PropTypes.string.isRequired,
      label: PropTypes.string.isRequired,
    }),
  ).isRequired,
  disabled: PropTypes.bool,
};

RadioButtons.defaultProps = {
  disabled: false,
  radioButtonsPerRow: 3,
};

export default RadioButtons;
