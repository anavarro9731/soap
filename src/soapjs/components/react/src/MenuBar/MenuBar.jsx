import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const MenuBar = props => {
  const mapMenuItemsToStyledMenuItems = menuItems =>
    menuItems.map(menuItem => (
      <S.MenuItem key={menuItem.menuItemId}>{menuItem.component}</S.MenuItem>
    ));

  return (
    <S.MenuBar>
      {props.brandComponent ? props.brandComponent : <div />}
      <S.MenuItems>
        {mapMenuItemsToStyledMenuItems(props.menuItems)}
      </S.MenuItems>
      <S.RightAlignedIcons>
        {mapMenuItemsToStyledMenuItems(props.rightAlignedItems)}
      </S.RightAlignedIcons>
    </S.MenuBar>
  );
};

MenuBar.propTypes = {
  brandComponent: PropTypes.element,
  menuItems: PropTypes.array,
  rightAlignedItems: PropTypes.array,
};

MenuBar.defaultProps = {
  menuItems: [],
  rightAlignedItems: [],
};

export default MenuBar;
