import styled from 'styled-components';

export const CheckboxWithLabel = styled.div`
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: center;
`;

export const Checkbox = styled.input`
  width: 25px;
  height: 25px;
  margin: 0px;
  ${props => !props.disabled && 'cursor: pointer;'}
`;

export const Label = styled.label`
  padding-${props => (props.labelOnLeftSide ? 'right' : 'left')}: 15px;
  white-space: nowrap;
  font-weight: bold;
  ${props => !props.disabled && 'cursor: pointer;'}
`;
