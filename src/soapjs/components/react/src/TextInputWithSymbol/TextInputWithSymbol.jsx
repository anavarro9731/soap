import React from 'react';
import PropTypes from 'prop-types';
import TextInput from '../TextInput';
import * as S from './style';

const TextInputWithSymbol = props => {
  const alignStylingProp = {
    rightAlignSymbol: props.rightAlignSymbol,
    symbolColour: props.symbolColour,
  };
  const symbol = <S.Symbol {...alignStylingProp}>{props.symbol}</S.Symbol>;
  return (
    <S.Grid {...alignStylingProp}>
      {!props.rightAlignSymbol && symbol}
      <TextInput {...props} type={props.type} border="none" />
      {props.rightAlignSymbol && symbol}
    </S.Grid>
  );
};

TextInputWithSymbol.propTypes = {
  rightAlignSymbol: PropTypes.bool,
  type: PropTypes.string,
  symbol: PropTypes.string.isRequired,
  symbolColour: PropTypes.string,
};

TextInputWithSymbol.defaultProps = {
  rightAlignSymbol: false,
  type: 'number',
  symbolColour: '#676767',
};

export default TextInputWithSymbol;
