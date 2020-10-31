import styled from 'styled-components';

export const MenuBar = styled.div`
  display: grid;
  grid-template-columns: 8% 68% 24%;
  align-items: center;
  background-color: #3b3f4a;
  height: 100%;
`;

export const MenuItems = styled.div`
  text-align: right;
  font-size: 1.05em;
`;

export const MenuItem = styled.div`
  padding: 0 15px;
  display: inline-block;
`;

export const RightAlignedIcons = styled.div`
  text-align: right;
  font-size: 1em;
`;
