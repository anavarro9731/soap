import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import * as S from './style';
import Tab from './Tab';

const checkChildIsTab = tab =>
  tab.type !== Tab &&
  console.error(
    'Only the Tab component should be used directly underneath the Tabs component',
  );

const mapChildrenToTabs = (
  children,
  setActiveTabIndex,
  activeTabIndex,
  useFullWidthForTabs,
) =>
  React.Children.map(children, (tab, tabIndex) => {
    // We don't want the title property on the divs
    const { title, ...tabProps } = tab.props;
    checkChildIsTab(tab);
    if (React.isValidElement(tab)) {
      const isActiveTab = activeTabIndex === tabIndex;

      return (
        <S.Tab
          useFullWidthForTabs={useFullWidthForTabs}
          isActiveTab={isActiveTab}
          onClick={() => setActiveTabIndex(tabIndex)}
          {...tabProps}
        >
          {tab.props.title}
        </S.Tab>
      );
    }
  });

const getTabContent = (children, activeTabIndex) =>
  React.Children.map(children, (tab, tabIndex) => {
    const isActiveTab = activeTabIndex === tabIndex;
    return <S.TabContent isActiveTab={isActiveTab}>{tab}</S.TabContent>;
  });

const setInitiallySelectedTab = (children, setActiveTabIndex) =>
  React.Children.map(children, (tab, tabIndex) => {
    if (tab.props.initiallySelected) {
      setActiveTabIndex(tabIndex);
    }
  });

const Tabs = props => {
  const [activeTabIndex, setActiveTabIndex] = useState(0);

  useEffect(() => {
    setInitiallySelectedTab(props.children, setActiveTabIndex);
  }, []);

  const tabs = mapChildrenToTabs(
    props.children,
    setActiveTabIndex,
    activeTabIndex,
    props.useFullWidthForTabs,
  );

  const tabContent = getTabContent(props.children, activeTabIndex);

  return (
    <S.TabsAndContent>
      <S.Tabs>{tabs}</S.Tabs>
      {tabContent}
    </S.TabsAndContent>
  );
};

Tabs.propTypes = {
  children: PropTypes.node.isRequired,
  useFullWidthForTabs: PropTypes.bool,
};

Tabs.defaultProps = {
  useFullWidthForTabs: false,
};

Tabs.Tab = Tab;

export default Tabs;
