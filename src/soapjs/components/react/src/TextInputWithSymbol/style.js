import styled from 'styled-components';
import {
  defaultLightBackgroundColour,
  defaultBorderColour,
} from '../style/defaults';

export const Grid = styled.div`
  display: grid;
  grid-template-columns: ${props =>
    props.rightAlignSymbol ? '1fr auto' : 'auto 1fr'};
  align-items: center;
  border: 1px solid ${defaultBorderColour};
  background-color: ${defaultLightBackgroundColour};
`;

export const Symbol = styled.div`
  padding: 6px;
  border-${props =>
    props.rightAlignSymbol
      ? 'left'
      : 'right'}: 1px solid ${defaultBorderColour};
  font-weight: bold;
  color: ${props => props.symbolColour};
  font-size: 18px;
`;
