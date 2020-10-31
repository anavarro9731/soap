import React from 'react';
import PropTypes from 'prop-types';
import { useFormikContext } from 'formik';

const Field = props => {
  const { component: Component, field, ...inputComponentSpecificProps } = props;

  const { setFieldValue } = useFormikContext();

  return (
    <Component
      name={field.name}
      value={field.value}
      onChange={newValue => setFieldValue(field.name, newValue)}
      onBlur={field.onBlur}
      {...inputComponentSpecificProps}
    />
  );
};

Field.propTypes = {
  field: PropTypes.object.isRequired,
  component: PropTypes.func.isRequired,
};

export default Field;
