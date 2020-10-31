import React from 'react';
import styled from 'styled-components';
import { isPositionLeft } from './positions';
import { Link as _ReactRouterLink } from 'react-router-dom';

export const Container = styled.div`
  width: fit-content;
  display: inline-block;
`;

export const Links = styled.div`
  display: grid;
  grid-template-columns: max-content;
  position: absolute;
   ${props => isPositionLeft(props.position) && 'transform: translateX(-80%);'}
  border: ${props => props.border};
`;

export const ReactRouterLink = styled(
  ({ colour, backgroundColour, hoverBackgroundColour, ...rest }) => (
    <_ReactRouterLink {...rest} />
  ),
)`
  color: ${props => props.colour};
  background-color: ${props => props.backgroundColour};
  padding: 5px 15px;
  text-decoration: none;

  &:hover {
    background-color: ${props => props.hoverBackgroundColour};
  }
`;

export const Link = styled.div`
  color: ${props => props.colour};
  background-color: ${props => props.backgroundColour};
  padding: 5px 15px;
  text-decoration: none;
  cursor: pointer;

  &:hover {
    background-color: ${props => props.hoverBackgroundColour};
  }
`;
