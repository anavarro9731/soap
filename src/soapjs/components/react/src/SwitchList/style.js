import styled from 'styled-components';

export const SwitchListVerticalMargin = styled.div`
  margin: 10px 0;
`;

export const Lists = styled.div`
  display: grid;
  grid-template-columns: ${props => props.listWidth} 100px ${props =>
      props.listWidth};
`;

export const ArrowButtons = styled.div`
  display: grid;
  grid-template-rows: 1fr 1fr;
  justify-items: center;
  align-items: center;
`;

export const ListTitle = styled.div`
  padding: 15px 0;
`;
