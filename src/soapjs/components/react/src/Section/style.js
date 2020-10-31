import styled from 'styled-components';

export const Heading = styled.div`
  display: grid;
  grid-template-columns: 75% 25%;
  padding-bottom: 15px;
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
  ${props => props.isSectionExpanded && 'padding-bottom: 15px;'}
`;

export const Section = styled.div`
  margin: 10px 0;
`;
