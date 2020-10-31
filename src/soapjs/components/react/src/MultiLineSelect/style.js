import styled from 'styled-components';

export const Select = styled.select`
  width: ${props => props.width};
  height: ${props => props.height};
  min-width: ${props => props.width};
  font-size: 16px;
  padding: 3px 0;

  &:focus {
    outline: none;
  }

  &:disabled {
    background-color: #ededed;
  }
`;

export const Option = styled.option`
  padding: 2px 5px;
`;
