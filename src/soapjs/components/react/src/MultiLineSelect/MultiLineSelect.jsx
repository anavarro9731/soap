import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const MultiLineSelect = props => (
  <S.Select
    multiple
    size={props.visibleOptions}
    ref={props.listRef}
    width={props.width}
    height={props.height}
    disabled={props.disabled}
  >
    {props.options.map(option => (
      <S.Option value={option.value} key={option.value}>
        {option.label}
      </S.Option>
    ))}
  </S.Select>
);

MultiLineSelect.propTypes = {
  visibleOptions: PropTypes.number,
  options: PropTypes.arrayOf(
    PropTypes.shape({
      value: PropTypes.string.isRequired,
      label: PropTypes.string.isRequired,
    }),
  ).isRequired,
  listRef: PropTypes.object,
  width: PropTypes.string,
  height: PropTypes.string,
};

MultiLineSelect.defaultProps = {
  visibleOptions: 15,
  width: '300px',
  height: '300px',
  disabled: false,
};

export default MultiLineSelect;
