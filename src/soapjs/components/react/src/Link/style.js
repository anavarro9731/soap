import React from 'react';
import styled from 'styled-components';
import { Link as ReactRouterLink } from 'react-router-dom';

export const Link = styled(({ textColour, hoverTextColour, ...rest }) => (
  <ReactRouterLink {...rest} />
))`
  color: ${props => props.textColour};
  height: ${props => props.height};
  text-decoration: none;

  &:hover {
    color: ${props => props.hoverTextColour};
  }
`;
