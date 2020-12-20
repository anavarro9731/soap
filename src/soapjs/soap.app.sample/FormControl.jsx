import React, {useEffect, useState} from 'react';
import {config, headerKeys, translate, useCommand, useQuery} from '@soap/modules';
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
import {SnackbarProvider, useSnackbar,} from 'baseui/snackbar';
import {Check} from "baseui/icon";
import {toaster, ToasterContainer} from 'baseui/toast';

function SoapFormControl(props) {

    const [query, setQuery] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isSubmitted, setIsSubmitted] = useState(false);
    const {handleSubmit, control, errors} = useForm();
    const {formEventName, afterSubmit} = props;

    useEffect(() => {
        if (isSubmitted) {
            window.scrollTo(0, 0);
        }
    }, [isSubmitted])

    const {enqueue} = useSnackbar();

    const c109v1_GetForm = {
        $type: 'Soap.Api.Sample.Messages.Commands.C109v1_GetForm, Soap.Api.Sample.Messages',
        c109_FormDataEventName: formEventName,
        headers: []
    };

    let formDataEvent = useQuery(query);

    if (!formDataEvent) return (<Button onClick={() => setQuery(c109v1_GetForm)}>Test</Button>);

    async function onSubmit(formValues) {

        try {

            const {e000_ValidationEndpoint: validationEndpoint} = formDataEvent;

            setIsSubmitting(true);

            if (config.logFormDetail) config.logger.log("FormValues", JSON.stringify(formValues, null, 2));

            mutateFormValuesIntoAnonymousCommandObject(formValues, formDataEvent);

            if (config.logFormDetail) config.logger.log("MessageFromCleansedFormValues", JSON.stringify(formValues, null, 2));

            const command = createRegisteredTypedMessageInstanceFromAnonymousObject(formValues);

            let response = await fetch(validationEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(command) // body data type must match "Content-Type" header
            });
            const msg = await response.text();

            if (msg) {
                toaster.warning(msg);
            } else {
                try {
                    useCommand(command);

                    if (!!afterSubmit) afterSubmit(command);

                    //* snackbar here
                    enqueue({
                        message: 'Form Submitted Successfully',
                        startEnhancer: ({size}) => <Check size={size}/>,
                    })
                } finally {
                    setIsSubmitted(true);
                }

            }
        } finally {
            setIsSubmitting(false);
        }

        function mutateFormValuesIntoAnonymousCommandObject(obj, formDataEvent) {
            
            cleanProperties(obj);
            
            const {
                e000_CommandName: commandAssemblyTypeName,
                e000_SasStorageTokenForCommand: sasToken,
                e000_CommandBlobId: blobId
            } = formDataEvent;

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
                        //* can be an array
                        if (value.isImage !== undefined && value.objectUrl !== undefined) {
                            //* it's a fileInfo object you need to convert to send just the id of the already uploaded blob which is what the backend expects
                            obj[key] = {id: value.id, name: value.name};
                        } else cleanProperties(value);
                    } else if (value === "") {
                        obj[key] = null; //* convert empty string used as default by react-hook-form back to null
                    }
                }
            }
        }
    }

    function onError(errors) {
        config.logger.log(JSON.stringify(errors, null, 2));
    }

    function renderField(fieldMeta) {
        /* react will not consider a control to be controlled until the value is not null or undefined
        therefore we set empty string, even on booleans [which are nulleable] a way to determine this. we then parse it and change
        back to null later if the user never set the value 
         */

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
        
        function fieldHasErrored(fieldName) {
            let fullPath = "errors";
            if (fieldName.includes('.')) {
                const parts = fieldName.split('.');
                for (const part of parts) {
                    fullPath += '?.' + part;
                }    
            } else {
                fullPath += '.' + fieldName;
            }
            
            let error = eval(fullPath);
            error = typeof error === 'object' ? 'required' : undefined;
            return error;
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
                                    label={fieldMeta.label}
                                    disabled={isSubmitted}
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
                                    disabled={isSubmitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <Input
                                        error={fieldHasErrored(fieldMeta.name)}
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
                                    disabled={isSubmitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <Textarea
                                        error={fieldHasErrored(fieldMeta.name)}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                    /></FormControl>
                            );
                        }}
                    />);
            case "number": //* longs will be rounded to ints if the user enters a float
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            const transform = {
                                input: (value) => {
                                    return isNaN(value) ? '' : value.toString(); 
                                }, // incoming input value
                                output: (e) => {
                                    const output = parseFloat(e.target.value);
                                    return isNaN(output) ? '' : output; // what's going to the final submit and store
                                } 
                            };
                            return (
                                <FormControl
                                    disabled={isSubmitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <Input
                                        error={fieldHasErrored(fieldMeta.name)}
                                        inputRef={ref}
                                        name={name}
                                        type="number"
                                        value={transform.input(value)}
                                        onChange={(v) => onChange(transform.output(v))}
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
                        render={({onChange, value}) => {
                            const transform = {
                                input: (value) => {
                                    if (value === null) {
                                        return value;
                                    } else if (typeof value === 'string') {
                                        return new Date(value)
                                    } else {
                                        throw 'Unexpected Date format';
                                    }
                                }, // incoming input value
                                output: (e) => {
                                    if (typeof e === 'object') {
                                        if (e.date === null) return null;
                                        else return e.date.toISOString();
                                    } else if (typeof e === 'string') {
                                        return e;
                                    } else {
                                        throw 'Unexpected Date format';
                                    }
                                }
                            };
                            return (
                                <FormControl
                                    disabled={isSubmitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <DatePicker
                                        error={fieldHasErrored(fieldMeta.name)}
                                        clearable
                                        value={transform.input(value)}
                                        onChange={v => onChange(transform.output(v))}
                                    />
                                </FormControl>);
                        }}
                    />);
            case "enumeration": {
                //* Note keys are UPPER CASE here since JSON.NET does not serialise objects (Enumeration) to pascal-case;
                convertObjectKeysToPascalCase(fieldMeta.initialValue);
                const allowMultiple = fieldMeta.initialValue.allowMultipleSelections;
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={{
                            allEnumerations: [],
                            selectedKeys: fieldMeta.initialValue.selectedKeys.map(x => x),
                            allowMultipleSelections: null
                        }}
                        rules={fieldMeta.required ? {validate: value => value.selectedKeys.length > 0} : {}}
                        render={({onChange, onBlur, value}) => {
                            const transform = {
                                input: (value) => {
                                    if (Array.isArray(value)) {
                                        return value;
                                    } else if (typeof value === 'object') {
                                        const result = value.selectedKeys.length > 0 ? value.selectedKeys.map(v => ({
                                            key: v,
                                            active: null,
                                            value: fieldMeta.initialValue.allEnumerations.find(e => e.key === v).value
                                        })) : [];
                                        return result;
                                    } else {
                                        throw 'Unexpected Enumeration format';
                                    }
                                }, // incoming input value
                                output: (value) => {
                                    if (Array.isArray(value)) {
                                        return {
                                            allEnumerations: [],
                                            selectedKeys: value.map(x => x.key),
                                            allowMultipleSelections: null
                                        }
                                    } else {
                                        throw 'Unexpected Enumeration format';
                                    }
                                }
                            };
                            return (
                                <FormControl
                                    disabled={isSubmitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <Select
                                        error={fieldHasErrored(fieldMeta.name)}
                                        options={fieldMeta.initialValue.allEnumerations}
                                        value={transform.input(value)}
                                        labelKey="value"
                                        multi={allowMultiple}
                                        valueKey="key"
                                        onChange={params => {
                                            onChange(transform.output(params.value))
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
                        render={({onChange, onBlur, value}) => {
                            /* don't transform inputs here, but only in clean function to allow browser to cache blob data during form usage
                            and then only transform back to blobmeta at the end which saves having to redownload the blob every render
                             */
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <FileUpload
                                        disabled={isSubmitted}
                                        error={fieldHasErrored(fieldMeta.name)}
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
                        render={({onChange, onBlur, value}) => {
                            /* don't transform inputs here, but only in clean function to allow browser to cache blob data during form usage
                            and then only transform back to blobmeta at the end which saves having to redownload the blob every render
                             */
                            return (
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}>
                                    <FileUpload
                                        disabled={isSubmitted}
                                        acceptedTypes=".jpg,.jpeg,.jfif,.png"
                                        onBlur={onBlur}
                                        //dimensions={{maxWidth:640,maxHeight:480}}
                                        error={fieldHasErrored(fieldMeta.name)}
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
        <ToasterContainer  autoHideDuration={3000}>
            <div>
                <H1> {translate(wordKeys.testFormHeader)} </H1>
                <form onSubmit={handleSubmit(onSubmit, onError)}>
                    {formDataEvent.e000_FieldData.map(field =>
                        (<ReactErrorBoundary key={field.name + "-errorB"}>
                            {renderField(field)}
                        </ReactErrorBoundary>)
                    )}
                    <Button
                        disabled={isSubmitted}
                        kind={KIND.primary}
                        isLoading={isSubmitting}>
                        Submit
                    </Button>
                </form>
                {renderDebug()}
            </div>
        </ToasterContainer>
    );
}


export default function SoapForm(props) {
    return (
        <SnackbarProvider>
            <SoapFormControl {...props} />
        </SnackbarProvider>
    );
}

