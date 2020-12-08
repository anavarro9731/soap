import React, {useState} from 'react';
import {translate, useQuery} from '@soap/modules';
import {Button, KIND} from 'baseui/button';
import {Input} from 'baseui/input'
import FileUpload from './FileUpload';
import {DatePicker} from 'baseui/datepicker';
import {Checkbox, LABEL_PLACEMENT, STYLE_TYPE} from 'baseui/checkbox'
import {Controller, useForm} from "react-hook-form";
import {FormControl} from "baseui/form-control";
import {H1} from 'baseui/typography';
import {ReactErrorBoundary} from "./ReactErrorBoundary";
import wordKeys from './translations/word-keys';
import {Select} from "baseui/select";
import {Textarea} from "baseui/textarea";

function SoapFormControl(props) {
    
    const [query, setQuery] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const {handleSubmit, control, errors } = useForm();

    const c109v1_GetForm = {
        $type: 'Soap.Api.Sample.Messages.Commands.C109v1_GetForm, Soap.Api.Sample.Messages',
        C109_FormDataEventName: props.formEventName,
        headers: []
    };

    let formDataEvent = useQuery(query);

    if (!formDataEvent) return (<Button onClick={() => setQuery(c109v1_GetForm)}>Test</Button>);

    function onSubmit(values) {
        setIsSubmitting(true);

        setTimeout(() => {
            console.log(JSON.stringify(values, null, 2));
            setIsSubmitting(false);
        }, 1000);
    }

    function onError(errors) {
        console.log(JSON.stringify(errors, null, 2));
    }

    function renderField(fieldMeta) {
        /* react will not consider a control to be controlled until the value is not null or undefined
        therefore we set empty string, even on booleans [which are nulleable] a way to determine this. we then parse it and change
        back to null later if the user never set the value 
         */
        function standardiseDateInputs(input) {
            let formattedDate;
            if (input === null) {
                formattedDate = input
            } else if (typeof input === 'string') {
                formattedDate = new Date(input)
            } else if (typeof input === 'object') {
                if (input.date === null) {
                    formattedDate = null
                } else {
                    formattedDate = new Date(input.date)
                }
            } else {
                throw 'Unexpected Date format';
            }
            return formattedDate;
        }

        switch (fieldMeta.dataType) {
            case "boolean":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        rules={{required: fieldMeta.required}}
                        /* using '' is a bit of hack to void a console warning/error from either react-hook-form
                        about the lack of a defaultValue which then means the field is not returned at all if you change
                        null to undefined or from react if you pass null because it then throws up about an uncontrolled component
                        being changed to a controlled component when you set a value which is because react considers any 
                        input with value === null as uncontrolled.
                         
                        In theory, for optional fields in general and here with optional booleans we would process a missing field fine as
                        the object passed to the constructor would not have the field and as long as its optional that would be fine,
                        but I don't want warnings or errors even if their harmless so i will manage the conversion from null to '' and back
                        myself. 
                         */
                        defaultValue={fieldMeta.initialValue ?? ''} 
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Checkbox
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        name={name}
                                        checked={value}
                                        isIndeterminate={value === ''}
                                        labelPlacement={LABEL_PLACEMENT.right}
                                        onChange={e => onChange(e.target.checked)}
                                        onBlur={onBlur}
                                    />
                                </FormControl>
                            );
                        }}
                    />);
            case "guid":
            case "string":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Input
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                    /></FormControl>
                            );
                        }}
                    />);
            case "multilinestring":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Textarea
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                    /></FormControl>
                            );
                        }}
                    />);
            case "number":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Input
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        inputRef={ref}
                                        name={name}
                                        type="number"
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                    /></FormControl>
                            );
                        }}
                    />);
            case "datetime":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <DatePicker
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        clearable
                                        value={standardiseDateInputs(value)}
                                        onChange={onChange}
                                    />
                                </FormControl>);
                        }}
                    />);
            case "enumeration":
                const allowMultiple = fieldMeta.initialValue.AllowMultipleSelections; 
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue.SelectedKeys.Length > 0 ? fieldMeta.initialValue.SelectedKeys : []}
                        rules={fieldMeta.required ? { validate: value => value.length > 0 } : {}}
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Select
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        options={fieldMeta.initialValue.AllEnumerations}
                                        value={value}
                                        labelKey="Value"
                                        multi={allowMultiple}
                                        valueKey="Key"
                                        onChange={params => {
                                            onChange(params.value)}}
                                        onBlur={onBlur}
                                    />
                                </FormControl>);
                        }}
                    />);
            case "file":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <FileUpload
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        value={value}
                                        onBlur={onBlur}
                                        onChange={onChange}
                                    />
                                </FormControl>);
                        }}
                    />);
            case "image":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <FileUpload
                                        onBlur={onBlur}
                                        //dimensions={{maxWidth:640,maxHeight:480}}
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        value={value}
                                        onChange={onChange}
                                    />
                                </FormControl>);
                        }}
                    />);    
        }
    }
    
    return (
        <div>
            <H1> {translate(wordKeys.testFormHeader)} </H1>
            <form onSubmit={handleSubmit(onSubmit, onError)}>
                {formDataEvent.e000_FieldData.map(field =>
                    (<ReactErrorBoundary key={field.name + "-errorB"}>
                        {renderField(field)}
                    </ReactErrorBoundary>)
                )}
                <Button kind={KIND.primary} isLoading={isSubmitting}>
                    Submit
                </Button>
            </form>
            <pre>{JSON.stringify(formDataEvent.e000_FieldData, undefined, 2)}</pre>
        </div>
    );
}

export default SoapFormControl;
