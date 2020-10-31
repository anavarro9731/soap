import React from 'react';
import PropTypes from 'prop-types';
import * as S from './style';

const ButtonPanel = props => {
  if (props.joinButtons) {
    const numberOfButtons = React.Children.count(props.children);

    return (
      <S.InlineButtons>
        {React.Children.map(props.children, (button, buttonIndex) => {
          const isFirstButton = buttonIndex === 0;
          const isLastButton = buttonIndex === numberOfButtons - 1;

          if (isFirstButton) {
            return React.cloneElement(button, { borderRadiusRight: 'none' });
          } else if (isLastButton) {
            return React.cloneElement(button, {
              borderRadiusLeft: 'none',
              noLeftBorder: true,
            });
          }
          return React.cloneElement(button, {
            borderRadiusLeft: 'none',
            borderRadiusRight: 'none',
            noLeftBorder: true,
          });
        })}
      </S.InlineButtons>
    );
  } else {
    return (
      <S.ButtonSpacing buttonSpacing={props.buttonSpacing}>
        {props.children}
      </S.ButtonSpacing>
    );
  }
};

ButtonPanel.propTypes = {
  children: PropTypes.node.isRequired,
  joinButtons: PropTypes.bool,
  buttonSpacing: PropTypes.string,
};

ButtonPanel.defaultProps = {
  joinButtons: false,
  buttonSpacing: '10px',
};

export default ButtonPanel;
