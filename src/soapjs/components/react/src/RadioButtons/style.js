import styled from 'styled-components';

export const Grid = styled.div`
  display: grid;
  grid-template-columns: repeat(${props => props.radioButtonsPerRow}, 1fr);
`;

export const MarginTopAndButton = styled.div`
  margin: 10px 0;
`;

export const RadioButton = styled.input`
  margin-right: 7px;
  cursor: pointer;
`;

export const Label = styled.label`
  cursor: pointer;
`;
