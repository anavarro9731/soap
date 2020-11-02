import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import {
  defaultLightTextColour,
  defaultPrimaryBackgroundColour,
} from '../style/defaults';
import { POSITIONS } from './positions';
import Button from '../Button';
import * as S from './style';

const renderLink = (option, props) => {
  const commonLinkProps = {
    key: option.label,
    colour: props.textColour,
    backgroundColour: props.backgroundColour,
    hoverBackgroundColour: props.hoverBackgroundColour,
  };

  return option.route ? (
    <S.ReactRouterLink {...commonLinkProps} to={option.route}>
      {option.label}
    </S.ReactRouterLink>
  ) : (
    <S.Link {...commonLinkProps} onClick={option.onClick}>
      {option.label}
    </S.Link>
  );
};

const DropdownButton = props => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const dropdownButtonRef = useRef();

  useEffect(() => {
    const handleClickOutside = event => {
      if (
        dropdownButtonRef.current &&
        !dropdownButtonRef.current.contains(event.target) &&
        isDropdownOpen
      ) {
        setIsDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  });

  const CustomButtonComponent = props.buttonComponent;

  return (
    <S.Container ref={dropdownButtonRef}>
      <CustomButtonComponent
        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
        {...props.additionalButtonProps}
      >
        {props.buttonText}
      </CustomButtonComponent>
      {isDropdownOpen && (
        <S.Links border={props.border} position={props.position}>
          {props.options.map(option => renderLink(option, props))}
        </S.Links>
      )}
    </S.Container>
  );
};

DropdownButton.propTypes = {
  buttonComponent: PropTypes.func,
  textColour: PropTypes.string,
  backgroundColour: PropTypes.string,
  border: PropTypes.string,
  hoverBackgroundColour: PropTypes.string,
  buttonText: PropTypes.node,
  options: PropTypes.arrayOf(
    PropTypes.shape({
      label: PropTypes.string.isRequired,
      route: PropTypes.string,
      onClick: PropTypes.func,
    }),
  ),
  position: PropTypes.string,
  additionalButtonProps: PropTypes.object,
};

DropdownButton.defaultProps = {
  buttonComponent: Button,
  textColour: defaultLightTextColour,
  backgroundColour: defaultPrimaryBackgroundColour,
  position: POSITIONS.RIGHT,
  additionalButtonProps: {},
  border: 'none',
};

DropdownButton.POSITIONS = POSITIONS;

export default DropdownButton;
