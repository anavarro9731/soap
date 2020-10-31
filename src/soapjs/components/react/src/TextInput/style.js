import styled from 'styled-components';

export const TextInput = styled.input`
  padding: 9px;
  box-sizing: border-box;
  width: ${props => props.width};
  border: ${props => props.border};
  ${props => props.disabled && `background-color: #ededed`}
  border-radius: 4px;
  font-size: 15px;

  &:focus {
    outline: none;
  }
`;
