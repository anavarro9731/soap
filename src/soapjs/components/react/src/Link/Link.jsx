import React from 'react';
import PropTypes from 'prop-types';
import { defaultTextColour } from '../../modules/src/style/defaults';
import * as S from './style';

const Link = props => (
  <S.Link
    // For react-router
    to={props.to}
    // For styled-components
    textColour={props.textColour}
    hoverTextColour={props.hoverTextColour}
    height={props.height}
  >
    {props.children}
  </S.Link>
);

Link.propTypes = {
  to: PropTypes.string.isRequired,
  children: PropTypes.node.isRequired,
  textColour: PropTypes.string,
  hoverTextColour: PropTypes.string,
  height: PropTypes.string,
};

Link.defaultProps = {
  textColour: defaultTextColour,
  hoverTextColour: defaultTextColour,
  height: 'auto',
};

export default Link;
