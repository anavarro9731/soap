import PropTypes from 'prop-types';
import {
  defaultTextColour,
  defaultLightTextColour,
  defaultHighlightColour,
} from '../../modules/src/style/defaults';

const Tab = props => {
  return props.children;
};

Tab.propTypes = {
  children: PropTypes.node,
  title: PropTypes.string.isRequired,
  tabTextColour: PropTypes.string,
  hoverAndActiveTabBackgroundColour: PropTypes.string,
  hoverAndActiveTabtextColour: PropTypes.string,
  initiallySelected: PropTypes.bool,
};

Tab.defaultProps = {
  tabTextColour: defaultTextColour,
  hoverAndActiveTabBackgroundColour: defaultHighlightColour,
  hoverAndActiveTabtextColour: defaultLightTextColour,
  initiallySelected: false,
};

export default Tab;
