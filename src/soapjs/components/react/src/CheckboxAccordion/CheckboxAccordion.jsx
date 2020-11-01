import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import Accordion from '../Accordion';
import Button from '../Button';
import ButtonPanel from '../ButtonPanel';
import Grid from '@material-ui/core/Grid';
import { translate, keys } from '../../modules/src/i18n';
import { useFormikContext } from 'formik';
import Checkbox from '../Checkbox';

const areAllCheckboxesChecked = checkboxes =>
  Object.values(checkboxes).every(checked => checked);

const mapCheckboxesToAccordionSections = (
  sections,
  checkboxes,
  selectSectionLabel,
  createFieldName,
  CheckboxComponent,
  FieldComponent,
  sectionCheckboxStates,
  toggleSectionCheckboxes,
) =>
  sections.map(section => ({
    id: section.value,
    title: section.label,
    content: (
      <Grid container>
        {checkboxes[section.value].map(checkbox => (
          <Grid key={`${section.value}.${checkbox.value}`} item xs={6} md={4}>
            <FieldComponent
              component={CheckboxComponent}
              name={createFieldName(section.value, checkbox.value)}
              checkboxLabel={checkbox.label}
            />
          </Grid>
        ))}
      </Grid>
    ),
    rightAlignedContent: (
      <CheckboxComponent
        value={sectionCheckboxStates[section.value] || false}
        onChange={toggleSectionCheckboxes(section.value)}
        checkboxLabel={selectSectionLabel || ''}
        labelOnLeftSide
      />
    ),
  }));

const CheckboxAccordion = props => {
  const [sectionCheckboxStates, setSectionCheckboxStates] = useState({});

  const updateSectionCheckboxValue = (section, checkboxValue) =>
    setSectionCheckboxStates({
      ...sectionCheckboxStates,
      [section]: checkboxValue,
    });

  const { values, setFieldValue } = useFormikContext();

  const createFieldName = (sectionName, checkboxFieldName) =>
    `${props.fieldGroupName}.${sectionName}.${checkboxFieldName}`;

  const setCheckboxFieldValuesForSection = (
    sectionCheckboxValues,
    sectionName,
    checkboxValue,
  ) =>
    Object.keys(sectionCheckboxValues).forEach(checkboxFieldName =>
      setFieldValue(
        createFieldName(sectionName, checkboxFieldName),
        checkboxValue,
      ),
    );

  const checkboxFieldValues = values[props.fieldGroupName];

  const toggleSelectAll = checkboxValue => {
    Object.keys(checkboxFieldValues).forEach(sectionName => {
      const sectionCheckboxValues = checkboxFieldValues[sectionName];

      setCheckboxFieldValuesForSection(
        sectionCheckboxValues,
        sectionName,
        checkboxValue,
      );
    });
  };

  const toggleSectionCheckboxes = sectionName => checkboxValue => {
    updateSectionCheckboxValue(sectionName, checkboxValue);

    const sectionCheckboxValues = checkboxFieldValues[sectionName];

    setCheckboxFieldValuesForSection(
      sectionCheckboxValues,
      sectionName,
      checkboxValue,
    );
  };

  useEffect(() => {
    let updatedSectionValues = {};
    Object.keys(checkboxFieldValues).forEach(sectionName => {
      const sectionCheckboxValues = checkboxFieldValues[sectionName];

      const allCheckboxesInSectionChecked = areAllCheckboxesChecked(
        sectionCheckboxValues,
      );

      updatedSectionValues[sectionName] = allCheckboxesInSectionChecked;
    });

    setSectionCheckboxStates(updatedSectionValues);
  }, [checkboxFieldValues]);

  const FieldComponent = props.fieldComponent;
  const ButtonComponent = props.buttonComponent;
  const CheckboxComponent = props.checkboxComponent;

  return (
    <React.Fragment>
      {!!props.sections.length && (
        <ButtonPanel>
          <ButtonComponent onClick={() => toggleSelectAll(true)}>
            {translate(keys.selectAll)}
          </ButtonComponent>
          <ButtonComponent onClick={() => toggleSelectAll(false)}>
            {translate(keys.deselectAll)}
          </ButtonComponent>
        </ButtonPanel>
      )}
      <Accordion
        sections={mapCheckboxesToAccordionSections(
          props.sections,
          props.checkboxes,
          props.selectSectionLabel,
          createFieldName,
          CheckboxComponent,
          FieldComponent,
          sectionCheckboxStates,
          toggleSectionCheckboxes,
        )}
      />
    </React.Fragment>
  );
};

CheckboxAccordion.propTypes = {
  fieldGroupName: PropTypes.string.isRequired,
  fieldComponent: PropTypes.func.isRequired,
  buttonComponent: PropTypes.func,
  checkboxComponent: PropTypes.func,
  selectSectionLabel: PropTypes.string,
  sections: PropTypes.arrayOf(
    PropTypes.shape({
      label: PropTypes.string.isRequired,
      value: PropTypes.string.isRequired,
    }),
  ),
  checkboxes: PropTypes.objectOf(
    PropTypes.arrayOf(
      PropTypes.shape({
        label: PropTypes.string.isRequired,
        value: PropTypes.string.isRequired,
      }),
    ),
  ),
};

CheckboxAccordion.defaultProps = {
  buttonComponent: Button,
  checkboxComponent: Checkbox,
};

export default CheckboxAccordion;
