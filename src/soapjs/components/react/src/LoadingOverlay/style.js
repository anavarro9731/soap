import styled, { keyframes } from 'styled-components';

const dots = keyframes`
      0%, 20% {
        color: #fdc798;
        text-shadow:
          .25em 0 0 #fdc798,
          .5em 0 0 #fdc798;
        }
      40% {
        color: #ff7d09;
        text-shadow:
          .25em 0 0 #fdc798,
          .5em 0 0 #fdc798;
        }
      60% {
        text-shadow:
          .25em 0 0 #ff7d09,
          .5em 0 0 #fdc798;
        }
      80%, 100% {
        text-shadow:
          .25em 0 0 #ff7d09,
          .5em 0 0 #ff7d09;
        }
`;

export const LoadingIcon = styled.div`
  height: 100%;
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;

  :after {
    color: #fdc798;
    font-size: 80px;
    content: ' .';
    font-family: 'Comic Sans MS';
    animation: ${dots} 1s steps(5, end) infinite;
  }
`;

export const HideChildren = styled.div`
  // contents means it behaves like the div doesn't exist
  display: ${props => (props.hide ? 'none' : 'contents')};
`;
