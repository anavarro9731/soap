import React from 'react';
import PropTypes from 'prop-types';
import Grid from '@material-ui/core/Grid';
import Button from '../Button';
import { translate, keys } from '../../modules/src/i18n';
import { FieldArray, useField } from 'formik';
import * as S from './style';

const RepeatableField = props => {
  const {
    component,
    buttonComponent,
    name,
    emptyField,
    hideAddButton,
    addButtonLabel,
    disabled,
    ...repeatableFieldSpecificProps
  } = props;

  const FieldComponent = component;
  const ButtonComponent = buttonComponent;

  const [field] = useField(name);

  const currentRepeatableFieldValues = field.value ? field.value : {};

  return (
    <S.VerticalMargin>
      <FieldArray name={name}>
        {({ push, remove }) => (
          <React.Fragment>
            <Grid container>
              {currentRepeatableFieldValues.map((fieldValues, index) => (
                <FieldComponent
                  key={index}
                  fieldValues={fieldValues}
                  repeatableFieldNamePrefix={`${name}[${index}]`}
                  disabled={disabled}
                  removeButton={
                    <ButtonComponent onClick={() => remove(index)}>
                      {translate(keys.remove)}
                    </ButtonComponent>
                  }
                  {...repeatableFieldSpecificProps}
                />
              ))}
            </Grid>
            {!hideAddButton && !disabled && (
              <ButtonComponent onClick={() => push(emptyField)}>
                {addButtonLabel}
              </ButtonComponent>
            )}
          </React.Fragment>
        )}
      </FieldArray>
    </S.VerticalMargin>
  );
};

RepeatableField.propTypes = {
  name: PropTypes.string.isRequired,
  component: PropTypes.func.isRequired,
  emptyField: PropTypes.oneOfType([PropTypes.string, PropTypes.object]),
  buttonComponent: PropTypes.func,
  addButtonLabel: PropTypes.string,
  hideAddButton: PropTypes.bool,
  disabled: PropTypes.bool,
};

RepeatableField.defaultProps = {
  buttonComponent: Button,
  addButtonLabel: translate(keys.add),
  hideAddButton: false,
  disabled: false,
};

export default RepeatableField;
