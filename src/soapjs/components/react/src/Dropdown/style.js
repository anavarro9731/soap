import styled from 'styled-components';
import { default as ReactSelect } from 'react-select';

export const Select = styled(ReactSelect)`
  width: ${props => props.width};
  float: ${props => props.float};
  min-width: ${props => props.minWidth};
`;
