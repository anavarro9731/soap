import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const getSelectedOptions = (selectedValues, options) =>
  options.filter(option => selectedValues.includes(option.value));

const MultipleSelectDropdown = props => {
  const reactSelectCustomStyling = {
    control: base => ({
      ...base,
      boxShadow: 'none',
      '&:focus': { borderColor: 'hsl(0,0%,80%)' },
      '&:hover': { borderColor: 'hsl(0,0%,80%)' },
      borderColor: 'hsl(0,0%,80%)',
    }),
  };

  const handleChange = options => {
    const currentSelectedOptions = options
      ? options.map(option => option.value)
      : [];
    props.onChange(currentSelectedOptions);
  };

  return (
    <S.Select
      // formik
      name={props.name}
      // react-select
      value={getSelectedOptions(props.value, props.options)}
      onBlur={props.onBlur}
      onChange={handleChange}
      options={props.options}
      styles={reactSelectCustomStyling}
      placeholder={props.placeholder}
      // styled-components
      width={props.width}
      float={props.float}
      minWidth={props.minWidth}
      isDisabled={props.disabled}
      isMulti
    />
  );
};

MultipleSelectDropdown.propTypes = {
  name: PropTypes.string,
  value: PropTypes.arrayOf(
    PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  ),
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

MultipleSelectDropdown.defaultProps = {
  width: '100%',
  float: 'none',
  minWidth: '0',
  disabled: false,
};

export default MultipleSelectDropdown;
