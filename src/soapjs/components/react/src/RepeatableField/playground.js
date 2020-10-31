import React from 'react';
import ReactDOM from 'react-dom';
import RepeatableField from './RepeatableField';
import { Formik, Form, useField } from 'formik';
import TextInput from '../TextInput';
import Button from '../Button';
import AppWrapper from '../AppWrapper';
import Field from '../Field';

const ExampleField = props => {
  const [field, meta] = useField(props.name);

  return <Field {...props} field={field} />;
};

const RepeatableInputField = props => (
  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr' }}>
    <ExampleField
      name={`${props.repeatableFieldNamePrefix}.country`}
      component={TextInput}
      width="400px"
    />
    <ExampleField
      name={`${props.repeatableFieldNamePrefix}.age`}
      component={TextInput}
      width="400px"
    />
    <span>{props.removeButton}</span>
  </div>
);

const emptyPersonField = { age: '', country: '' };

const personFieldValues = [
  { age: '10', country: 'United Kingdom' },
  { age: '12', country: 'France' },
];

ReactDOM.render(
  <AppWrapper useDefaultFont>
    <Formik
      onSubmit={formValues => console.log(formValues)}
      initialValues={{
        testName: personFieldValues.length
          ? personFieldValues
          : emptyPersonField,
      }}
    >
      {({ values }) => (
        <Form>
          <RepeatableField
            name="testName"
            component={RepeatableInputField}
            currentFormValues={values}
            emptyField={emptyPersonField}
          />
          <div style={{ height: '10px' }} />
          <Button type="submit">Console.log form values</Button>
        </Form>
      )}
    </Formik>
  </AppWrapper>,
  document.getElementById('content'),
);
