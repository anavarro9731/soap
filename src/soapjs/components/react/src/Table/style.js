import styled from 'styled-components';

export const Table = styled.div`
  display: grid;
  grid-template-columns: ${props => props.columnWidths};
`;

export const Cell = styled.div`
  outline: 1px solid ${props => props.borderColour};
  color: ${props => props.textColour};
  background-color: ${props => props.backgroundColour};
  display: flex;
  align-items: center;
  box-sizing: border-box;
  overflow: hidden;
  white-space: nowrap;
  ${props => props.fontWeight && `font-weight: ${props.fontWeight}`}
`;

export const CellContent = styled.span`
  overflow: hidden;
  text-overflow: ellipsis;
  padding: ${props => props.padding};
`;
