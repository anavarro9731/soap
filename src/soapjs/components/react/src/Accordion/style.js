import styled from 'styled-components';

export const AccordionItem = styled.div`
  padding: 12px 0;
  ${props => props.withBorder && 'border-bottom: 1px solid #d2d2d2;'}
`;

export const Heading = styled.div`
  display: grid;
  grid-template-columns: 75% 25%;
`;

export const ClickToToggleExpand = styled.div`
  display: grid;
  grid-template-columns: 50px 1fr;
  align-items: center;
  cursor: pointer;
`;

export const Chevron = styled.img`
  width: 17px;
  height: 17px;
  margin: 0 auto;
`;

export const Title = styled.div`
  font-size: 16px;
  cursor: pointer;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
`;

export const RightAlignedContent = styled.div`
  justify-self: right;
  padding-right: 30px;
`;

export const Content = styled.div`
  font-size: 14px;
  word-wrap: break-word;
  padding: 0 60px;
  padding-top: 5px;
`;
