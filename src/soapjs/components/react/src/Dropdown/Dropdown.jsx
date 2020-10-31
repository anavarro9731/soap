import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const Dropdown = props => {
  const findSelectedValue = selectedOptionValue =>
    props.options.find(option => option.value === selectedOptionValue);

  const reactSelectCustomStyling = {
    control: base => ({
      ...base,
      boxShadow: 'none',
      '&:focus': { borderColor: 'hsl(0,0%,80%)' },
      '&:hover': { borderColor: 'hsl(0,0%,80%)' },
      borderColor: 'hsl(0,0%,80%)',
    }),
  };

  return (
    <S.Select
      // formik
      name={props.name}
      // react-select
      value={findSelectedValue(props.value)}
      onBlur={props.onBlur}
      onChange={option => props.onChange(option.value)}
      options={props.options}
      styles={reactSelectCustomStyling}
      placeholder={props.placeholder}
      // styled-components
      width={props.width}
      float={props.float}
      minWidth={props.minWidth}
      isDisabled={props.disabled}
    />
  );
};

Dropdown.propTypes = {
  name: PropTypes.string,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  onChange: PropTypes.func,
  onBlur: PropTypes.func,
  options: PropTypes.arrayOf(
    PropTypes.shape({
      value: PropTypes.oneOfType([PropTypes.number, PropTypes.string])
        .isRequired,
      label: PropTypes.string.isRequired,
    }),
  ).isRequired,
  width: PropTypes.string,
  float: PropTypes.string,
  minWidth: PropTypes.string,
  placeholder: PropTypes.string,
  disabled: PropTypes.bool,
};

Dropdown.defaultProps = {
  width: '100%',
  float: 'none',
  minWidth: '0',
  disabled: false,
};

export default Dropdown;
