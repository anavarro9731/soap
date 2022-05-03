import React, {useEffect, useRef, useState} from 'react';
import {useQuery} from '../hooks/useQuery';
import {useCommand} from '../hooks/useCommand';
import config from '../soap/config';
import {createRegisteredTypedMessageInstanceFromAnonymousObject, headerKeys} from '../soap/messages';
import {Button, KIND} from 'baseui/button';
import {Input, MaskedInput} from 'baseui/input'
import {FileUpload} from './FileUpload';
import {DatePicker} from 'baseui/datepicker';
import {Checkbox, LABEL_PLACEMENT} from 'baseui/checkbox'
import {Controller, useForm} from "react-hook-form";
import {FormControl} from "baseui/form-control";
import {Label3} from 'baseui/typography';
import { ReactErrorBoundary } from "./ReactErrorBoundary";
import {Select} from "baseui/select";
import {Textarea} from "baseui/textarea";
import {useSnackbar,} from 'baseui/snackbar';
import {Check} from "baseui/icon";
import {toaster} from 'baseui/toast';
import JoditEditor from "jodit-react";
import {optional, types, validateArgs} from "../soap/util";


export function AutoForm(props) {

    const {afterSubmit, query, sendQuery = true, afterCancel, cancelText, submitText, hiddenFields = []} = props;

    validateArgs(
        [{afterSubmit}, types.function, optional],
        [{query}, types.object],
        [{sendQuery}, types.boolean],
        [{afterCancel}, types.function, optional],
        [{cancelText}, types.string, optional],
        [{submitText}, types.string, optional],
        [{hiddenFields}, [types.string], optional]
    )

    //* jodit editor
    const editor = useRef(null)
    const [content, setContent] = useState('')

    //* react-hook-form
    const {handleSubmit, control, errors} = useForm();  //* errors is used in eval

    const {enqueue} = useSnackbar();

    const [showLoader, setShowLoader] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [submitSucceeded, setSubmitSucceeded] = useState(false);

    //* used as a pair, to cause the sending of the command when the user submits
    const [command, setCommand] = useState(undefined);
    const [sendCommand, setSendCommand] = useState(false);

    const isInitialSubmit = (sendCommand && submitted === false);

    useEffect(() => {
        if (submitted) {
            window.scrollTo(0, 0);
        }
    }, [submitted])

    //* triggered immediately
    const [formDataEvent, refresh] = useQuery({query, sendQuery});

    useEffect(() => {

        if (submitSucceeded) {
            /* we don't run this code in the click handler because we don't know if the command.handle call
            succeeded yet
             */

            /* queue the snackbar, this causes a rerender and that would throw
            a react error because you cannot rerender snackbar (which is a parent component) 
            while rendering autoform so this is needs to be in a useEffect rather than it
            the in try block after sending
             */
            enqueue({
                message: 'Form Submitted Successfully',
                startEnhancer: ({size}) => <Check size={size}/>,
            });

            //* if there is a post submit hook run it
            if (!!afterSubmit) {
                afterSubmit(command);
            }
        }

    }, [submitSucceeded]);

    try {
        //* once the user has submitted the form in the onclick handler then it will set sendCommand and the hook will send
        useCommand(command, sendCommand);

        if (isInitialSubmit) {
            setSubmitSucceeded(true);
        }

    } finally {
        if (isInitialSubmit) { //* without isInitialSubmit you get an infinite loop after submitting
            //* only submit it once, even if it fails
            setSubmitted(true);
        }
    }

    //* needs to be at the end as early termination would change the hooks that are run (its like an if else)
    if (!formDataEvent) return null;

    return (
        <div>
            <form onSubmit={handleSubmit(onSubmit, onError)}>
                {formDataEvent.e000_FieldData.map(field =>
                    (<ReactErrorBoundary key={field.name + "-errorB"}>
                        {renderField(field)}
                    </ReactErrorBoundary>)
                )}
                {afterCancel ? (<Button
                    disabled={submitted}
                    type="button"
                    style={{marginRight:"10px"}}
                    onClick={afterCancel}
                    kind={KIND.primary}>
                    {cancelText ?? "Cancel"}
                </Button>) : null}
                <Button
                    disabled={submitted}
                    kind={KIND.primary}
                    isLoading={showLoader}>
                    {submitText ?? "Submit"}
                </Button>
            </form>
            {renderDebug()}
        </div>
    );


    async function onSubmit(formValues) {

        try {

            setShowLoader(true);

            if (config.logFormDetail) config.logger.log("FormValues", JSON.stringify(formValues, null, 2));

            mutateFormValuesIntoAnonymousCommandObject(formValues, formDataEvent);

            if (config.logFormDetail) config.logger.log("MessageFromCleansedFormValues", JSON.stringify(formValues, null, 2));

            const command = createRegisteredTypedMessageInstanceFromAnonymousObject(formValues);

            const validationEndpoint = `${config.vars.functionAppRoot}/ValidateMessage?type=${encodeURIComponent(command.$type)}`;

            let response = await fetch(validationEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(command) // body data type must match "Content-Type" header
            });
            const msg = await response.text();

            if (msg) {
                const messages = msg.split('\r\n');
                toaster.negative(messages.map(m => (<Label3 color="primaryB" key={m}>{m}</Label3>)));
                return;
            }

            setCommand(command);
            setSendCommand(true);

        } finally {
            setShowLoader(false);
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
                fullPath += '?.' + fieldName;
            }
            
            let error = new Function('fullPath','errors', `return ${fullPath};`)(fullPath, errors);
            
            error = typeof error === 'object' ? 'required' : undefined;
            return error;
        }

        
        const isHidden = hiddenFields.includes(fieldMeta.propertyName) || fieldMeta.options.hidden === true;
        const inlineStyle = isHidden ? {display: "none"} : undefined;
        
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
                                <div style={inlineStyle}>
                                <FormControl
                                    label={fieldMeta.label}
                                    disabled={submitted}
                                    style={inlineStyle}
                                >
                                    <Checkbox
                                        name={name}
                                        checked={value}
                                        labelPlacement={LABEL_PLACEMENT.right}
                                        onChange={e => onChange(e.target.checked)}
                                        onBlur={onBlur}
                                    />
                                </FormControl>
                                </div>
                            );
                        }}
                    />);
            case "guid":

                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={fieldMeta.required ? {validate: value => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value)} : {}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <MaskedInput
                                        error={fieldHasErrored(fieldMeta.name)}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                        placeholder="Unique Identifier (e.g. ac769a1f-e681-4bc4-8098-45e0b4d923cf)"
                                        mask={"********-****-****-****-************"}
                                        maskChar="*"
                                    /></FormControl>
                                </div>
                            );
                        }}
                    />);

            case "string":
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <Input
                                        error={fieldHasErrored(fieldMeta.name)}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                    /></FormControl>
                                </div>
                            );
                        }}
                    />);
            case "joditeditor":
                /* all options from 
                 https://xdsoft.net/jodit/doc/                 
                 */
                const config = {
                    readonly: false,
                    height: fieldMeta.options.height,
                    uploader: {
                        insertImageAsBase64URI: true
                    },
                    buttons: "bold,italic,underline, strikethrough, eraser, superscript, ul, ol, indent, outdent, left, font, fontsize, paragraph, brush, image, copyformat, cut, copy, paste, selectall, hr, table, link, symbol, undo, redo, find, preview, print"
                }
                return (
                    <Controller
                        control={control}
                        name={fieldMeta.name}
                        defaultValue={fieldMeta.initialValue ?? ''}
                        rules={{required: fieldMeta.required}}
                        render={({onChange, onBlur, value, name, ref}) => {
                            return (
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}>
                                    <JoditEditor
                                        ref={editor}
                                        value={value}
                                        config={config}
                                        onBlur={onBlur} // preferred to use only this option to update the content for performance reasons
                                        onChange={onChange}
                                    />
                                </FormControl>
                                </div>
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
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <Textarea
                                        error={fieldHasErrored(fieldMeta.name)}
                                        inputRef={ref}
                                        name={name}
                                        value={value}
                                        onChange={onChange}
                                        onBlur={onBlur}
                                        overrides={{
                                            Input: {
                                                style: {
                                                    maxHeight: '800px',
                                                    minHeight: (fieldMeta.options.height > 0 ? fieldMeta.options.height : 100) + 'px',
                                                },
                                            },
                                            InputContainer: {
                                                style: {
                                                },
                                            },
                                        }}
                                    /></FormControl>
                                </div>
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
                                <div style={inlineStyle}>
                                    <FormControl
                                        disabled={submitted}
                                        label={fieldMeta.label} caption={fieldMeta.caption}
                                        error={fieldHasErrored(fieldMeta.name)}

                                    >
                                        <Input
                                            error={fieldHasErrored(fieldMeta.name)}
                                            inputRef={ref}
                                            name={name}
                                            type="number"
                                            value={transform.input(value)}
                                            onChange={(v) => onChange(transform.output(v))}
                                            onBlur={onBlur}
                                        /></FormControl>
                                </div>

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
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <DatePicker
                                        error={fieldHasErrored(fieldMeta.name)}
                                        clearable
                                        value={transform.input(value)}
                                        onChange={v => onChange(transform.output(v))}
                                    />
                                </FormControl>
                                </div>
                            );
                        }}
                    />);
            case "enumeration": {
                //* Note keys are UPPER CASE here since JSON.NET does not serialise objects (Enumeration) to pascal-case;
                convertObjectKeysToPascalCase(fieldMeta.initialValue);
                const allowMultiple = fieldMeta.initialValue.allowMultipleSelections;
                const creatable = fieldMeta.options.creatable;
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
                                            value: creatable ? v : fieldMeta.initialValue.allEnumerations.find(e => e.key === v).value
                                        })) : [];
                                        return result;
                                    } else {
                                        throw 'Unexpected Enumeration format';
                                    }
                                }, // incoming input value
                                output: (value) => {

                                    if (Array.isArray(value)) {
                                        return {
                                            allEnumerations: [], //* clear this down we don't want to pass it back
                                            selectedKeys: value.map(x => x.key),
                                            allowMultipleSelections: null //* same here, inbound only
                                        }
                                    } else {
                                        throw 'Unexpected Enumeration format';
                                    }
                                }
                            };
                            return (
                                <div style={inlineStyle}>
                                <FormControl
                                    disabled={submitted}
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <Select
                                        error={fieldHasErrored(fieldMeta.name)}
                                        creatable={creatable}
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
                                </FormControl>
                                </div>
                                );
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
                                <div style={inlineStyle}>
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <FileUpload
                                        disabled={submitted}
                                        error={fieldHasErrored(fieldMeta.name)}
                                        value={value}
                                        onBlur={onBlur}
                                        onChange={onChange}
                                    />
                                </FormControl>
                                </div>
                            );
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
                                <div style={inlineStyle}>
                                <FormControl
                                    label={fieldMeta.label} caption={fieldMeta.caption}
                                    error={fieldHasErrored(fieldMeta.name)}
                                    style={inlineStyle}
                                >
                                    <FileUpload
                                        disabled={submitted}
                                        acceptedTypes=".jpg,.jpeg,.jfif,.png"
                                        onBlur={onBlur}
                                        //dimensions={{maxWidth:640,maxHeight:480}}
                                        error={fieldHasErrored(fieldMeta.name)}
                                        value={value}
                                        onChange={onChange}
                                    />
                                </FormControl>
                                </div>
                            );
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

}

