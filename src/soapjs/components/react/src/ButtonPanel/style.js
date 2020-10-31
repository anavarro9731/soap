import styled from 'styled-components';

export const InlineButtons = styled.div`
  display: inline-block;
`;

export const ButtonSpacing = styled.div`
  & > * {
    margin-right: ${props => props.buttonSpacing};
  }

  & > *:last-child {
    margin-right: 0;
  }
`;
