import styled from 'styled-components';

export const Button = styled.button`
  background-color: ${props =>
    props.disabled ? props.disabledBackgroundColour : props.backgroundColour};
  cursor: pointer;
  color: ${props => props.colour};
  padding: ${props => `${props.verticalPadding} ${props.horizontalPadding}`};
  font-size: ${props => props.fontSize};
  border: ${props => props.border};
  border-top-left-radius: ${props => props.borderRadiusLeft};
  border-bottom-left-radius: ${props => props.borderRadiusLeft};
  border-top-right-radius: ${props => props.borderRadiusRight};
  border-bottom-right-radius: ${props => props.borderRadiusRight};
  ${props => props.noLeftBorder && 'border-left: none;'}
  height: ${props => props.height};
  width: ${props => props.width};
  ${props => props.disabled && 'pointer-events: none;'}

  &:focus {
    outline: none;
  }

  ${props =>
    props.hoverBackgroundColour &&
    `&:hover {
    background-color: ${props.hoverBackgroundColour};
  }`}
`;
