import React, {useState} from 'react';
import {config, translate, useCommand, useQuery} from '@soap/modules';
import {Button, KIND} from 'baseui/button';
import {Input} from 'baseui/input'
import FileUpload from './FileUpload';
import {DatePicker} from 'baseui/datepicker';
import {Checkbox, LABEL_PLACEMENT} from 'baseui/checkbox'
import {Controller, useForm} from "react-hook-form";
import {FormControl} from "baseui/form-control";
import {H1} from 'baseui/typography';
import {ReactErrorBoundary} from "./ReactErrorBoundary";
import wordKeys from './translations/word-keys';
import {Select} from "baseui/select";
import {Textarea} from "baseui/textarea";
import {createRegisteredTypedMessageInstanceFromAnonymousObject} from '@soap/modules/lib/soap/messages';


function SoapFormControl(props) {

    const [query, setQuery] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const {handleSubmit, control, errors} = useForm();

    const c109v1_GetForm = {
        $type: 'Soap.Api.Sample.Messages.Commands.C109v1_GetForm, Soap.Api.Sample.Messages',
        c109_FormDataEventName: props.formEventName,
        headers: []
    };

    let formDataEvent = useQuery(query);
    
    if (!formDataEvent) return (<Button onClick={() => setQuery(c109v1_GetForm)}>Test</Button>);

    function onSubmit(formValues) {

        try {
            setIsSubmitting(true);

            if (config.logFormDetail) config.logger.log("FormValues", JSON.stringify(formValues, null, 2));

            mutateFormValuesIntoAnonymousCommandObject(formValues, formDataEvent);
            
            const command = createRegisteredTypedMessageInstanceFromAnonymousObject(formValues);
            
            if (config.logFormDetail) config.logger.log("MessageFromCleansedFormValues", JSON.stringify(formValues, null, 2));
            
            useCommand(command);

        } finally {
            setIsSubmitting(false);
        }


        function mutateFormValuesIntoAnonymousCommandObject(obj, formDataEvent) {

            cleanProperties(obj);
            
            const {e000_CommandName: commandAssemblyTypeName, e000_SasStorageTokenForCommand: sasToken, e000_CommandBlobId: blobId } = formDataEvent;

            obj.$type = commandAssemblyTypeName;  //* set root type
            //* add blob headers (the rest are added by command handler)
            obj.headers = [
                {active: null, key: headerKeys.sasStorageToken, value: sasToken},
                {active: null, key: headerKeys.blobId, value: blobId}
            ];
            
            function cleanProperties(obj) {

                for (const key in obj) {

                    const value = obj[key];
                    if (typeof value === "object" && value !== null) {

                        if (Array.isArray(value)) {
                            if (value.length > 0 && value[0].active !== undefined && value[0].value !== undefined && value[0].key !== undefined) {
                                //* its an array of enumerations, you need to convert it to EnumerationAndFlags for the round trip
                                obj[key] = {
                                    allEnumerations: [],
                                    selectedKeys: value.map(x => x.key),
                                    allowMultipleSelections: null
                                };
                            } else {
                                cleanProperties(value);
                            }
                        } else {
                            if (value.isImage !== undefined && value.objectUrl !== undefined) {
                                //* it's a fileInfo object you need to convert to send just the id of the already uploaded blob which is what the backend expects
                                obj[key] = { id: value.id, name : value.name };
                            }
                            cleanProperties(value);
                        }
                    } else if (value === "") {
                        obj[key] = null; //* convert empty string used as default by react-hook-form back to null
                    }
                }
            }
        }
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

        function convertObjectKeysToPascalCase(obj) {
            for (const key in obj) {

                const newKey = key.charAt(0).toLowerCase() + key.substring(1);
                if (newKey !== key || Array.isArray(obj)) { 
                    if (!Array.isArray(obj)) { //* convert it's property names
                        //* convert it first or you'll end up deleting its converted children
                        Object.defineProperty(obj, newKey, Object.getOwnPropertyDescriptor(obj, key)); //* create new
                        delete obj[key]; //* delete old
                    }

                    if (typeof obj[newKey] === "object" && obj[newKey] !== null) {
                        //* convert it's children (objects and arrays caught here)
                        convertObjectKeysToPascalCase(obj[newKey]);
                    }
                }

            }
        }

        switch (fieldMeta.dataType) {
            case "boolean":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? false} /* the backend supports nullable booleans, but I don't have a good way to handle that in teh frontend
                        it's probably going to confuse people, if I could change the icon for isIndeterminate to a '?' that may be a solution in future, for now we will always default to false
                        and lose the nullable option in the frontend also false is considered failing validation so checkbox can't be required either, i.e. its always valid.
                        
                        /* if you go back to allowing nullable boolean using '' as default value is a bit of hack to void a console warning/error from either react-hook-form
                        about null as a defaultValue which then means the field is not returned at all. 
                        if you use null anyway or change null to undefined to avoid the default value warning it then throws up about an uncontrolled component being changed to a 
                        controlled component when you set a value which happens because react considers any input with value === null as uncontrolled.
                         
                        In theory, for optional fields in general and here with optional booleans we would process a missing field fine as the object passed to the command constructor 
                        would not have the field and as long as its optional that would be fine, but I don't want warnings or errors even if their harmless so i would choose to 
                        convert null to '' and then manage the conversion from '' back to null myself. Datepicker seems to work with null values ok, so I don't really understand
                        the guts of react-hook-form in relation to null defaultValues maybe some further research is required.
                         */
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                >
                                    <Checkbox
                                        name={name}
                                        checked={value}
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
            case "enumeration": {
                //* Note keys are UPPER CASE here since JSON.NET does not serialise objects (Enumeration) to pascal-case;
                convertObjectKeysToPascalCase(fieldMeta.initialValue);
                const allowMultiple = fieldMeta.initialValue.allowMultipleSelections;
                const defaultValue = fieldMeta.initialValue.selectedKeys.length > 0 ? fieldMeta.initialValue.selectedKeys.map(v => ({
                    key: v,
                    active: null,
                    value: fieldMeta.initialValue.allEnumerations.find(e => e.key === v).value
                })) : [];
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={defaultValue}
                        rules={fieldMeta.required ? {validate: value => value.length > 0} : {}}
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <Select
                                        error={Object.keys(errors).includes(name) ? "required" : undefined}
                                        options={fieldMeta.initialValue.allEnumerations}
                                        value={value}
                                        labelKey="value"
                                        multi={allowMultiple}
                                        valueKey="key"
                                        onChange={params => {
                                            onChange(params.value)
                                        }}
                                        onBlur={onBlur}
                                    />
                                </FormControl>);
                        }}
                    />);
            }
            case "file": {
                //* Note keys are UPPER CASE here since JSON.NET does not serialise objects (Base64Blob) to pascal-case;
                convertObjectKeysToPascalCase(fieldMeta.initialValue);
                const defaultValue = fieldMeta.initialValue ? fieldMeta.initialValue : null;
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={defaultValue}
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
            }
            case "image": {
                //* Note keys are UPPER CASE here since JSON.NET does not serialise objects (Base64Blob) to pascal-case;
                convertObjectKeysToPascalCase(fieldMeta.initialValue);
                const defaultValue = fieldMeta.initialValue ? fieldMeta.initialValue : null;
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={defaultValue}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name}) => {
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={Object.keys(errors).includes(name) ? "required" : undefined}>
                                    <FileUpload
                                        acceptedTypes=".jpg,.jpeg,.jfif,.png"
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
    }

    function renderDebug() {
        const debug = (<pre>{JSON.stringify(formDataEvent, undefined, 2)}</pre>);
        if (config.logFormDetail) {
            return debug;
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
            {renderDebug()}
        </div>
    );
}

export default SoapFormControl;
