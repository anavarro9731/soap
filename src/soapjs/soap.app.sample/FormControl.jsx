import React from 'react';
import {translate, useQuery} from '@soap/modules';
import wordKeys from './translations/word-keys';


function FormControl(props) {

    const c109v1_GetForm = {
        $type: 'Soap.Api.Sample.Messages.Commands.C109v1_GetForm, Soap.Api.Sample.Messages',
        C109_FormDataEventName: props.formEventName,
        headers: []
    };

    const formDataEvent = useQuery(c109v1_GetForm);


    if (!formDataEvent) return (<h1>Loading...</h1>);

    return (
        <div>
            <h1>{translate(wordKeys.testFormHeader)}</h1>
            <pre>{JSON.stringify(formDataEvent, undefined, 2)}</pre>

        </div>
    );
}

export default FormControl;
