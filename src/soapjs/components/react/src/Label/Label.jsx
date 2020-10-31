import PropTypes from 'prop-types';

const Label = props => props.value || null;

Label.propTypes = {
  value: PropTypes.node,
};

export default Label;
