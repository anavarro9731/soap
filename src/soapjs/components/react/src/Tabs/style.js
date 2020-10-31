import styled from 'styled-components';

export const TabsAndContent = styled.div`
  width: 100%;
  height: 100%;
  display: grid;
  grid-template-rows: auto 1fr;
`;

export const Tabs = styled.div`
  width: 100%;
  text-align: center;
  display: table;
  table-layout: fixed;
  border-collapse: collapse;
  box-sizing: border-box;
`;

export const Tab = styled.div`
  vertical-align: middle;
  display: ${props =>
    props.useFullWidthForTabs ? 'table-cell' : 'inline-block'};
  text-align: center;
  padding: 10px;
  width: 12%;
  height: 100%;
  cursor: pointer;
  border: 1px solid #d2d2d2;
  border-bottom: none;
  margin-right: -1px;
  margin-bottom: -1px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: ${props =>
    props.isActiveTab
      ? props.hoverAndActiveTabtextColour
      : props.tabTextColour};
  ${props =>
    props.isActiveTab &&
    `background-color: ${props.hoverAndActiveTabBackgroundColour};
    `}

  :hover {
    background-color: ${props => props.hoverAndActiveTabBackgroundColour};
    color: ${props => props.hoverAndActiveTabtextColour};
  }
`;

export const TabContent = styled.div`
  border: 1px solid #d2d2d2;
  ${props => !props.isActiveTab && `display: none;`}
  padding: 10px;
`;
