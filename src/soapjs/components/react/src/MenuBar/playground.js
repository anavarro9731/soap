import React from 'react';
import ReactDOM from 'react-dom';
import MenuBar from './MenuBar';
import KurveSmileIcon from 'url:../style/images/kurve-smile.png';
import AppWrapper from '../AppWrapper';
import styled from 'styled-components';

const BrandImageLink = styled.a`
  height: 100%;
`;

const BrandImage = styled.div`
  background-image: url(${props => props.imageUrl});
  background-repeat: no-repeat;
  background-position: center;
  height: 100%;
`;

const brandComponent = (
  <BrandImageLink href="/">
    <BrandImage imageUrl={KurveSmileIcon} />
  </BrandImageLink>
);

ReactDOM.render(
  <AppWrapper>
    <div style={{ height: '50px' }}>
      <MenuBar
        brandComponent={brandComponent}
        menuItems={[
          {
            menuItemId: 'importFile',
            component: (
              <a style={{ color: 'white' }} href="">
                Import File
              </a>
            ),
          },
          {
            menuItemId: 'importProducts',
            component: (
              <a style={{ color: 'white' }} href="">
                Import Products
              </a>
            ),
          },
          {
            menuItemId: 'configValidation',
            component: (
              <a style={{ color: 'white' }} href="">
                Config Validation
              </a>
            ),
          },
          {
            menuItemId: 'syncHistory',
            component: (
              <a style={{ color: 'white' }} href="">
                Sync History
              </a>
            ),
          },
          {
            menuItemId: 'editProducts',
            component: (
              <a style={{ color: 'white' }} href="">
                Edit Products
              </a>
            ),
          },
        ]}
        rightAlignedItems={[
          {
            menuItemId: 'heldProducts',
            component: (
              <a style={{ color: 'white' }} href="">
                Held Products
              </a>
            ),
          },
          {
            menuItemId: 'archivedProducts',
            component: (
              <a style={{ color: 'white' }} href="">
                Archived Products
              </a>
            ),
          },
        ]}
      />
    </div>
  </AppWrapper>,
  document.getElementById('content'),
);
