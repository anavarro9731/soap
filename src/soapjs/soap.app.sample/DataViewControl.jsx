import React from 'react';
import {useQuery} from '@soap/modules';
import {withStyle} from 'baseui';
import {StyledSpinnerNext} from 'baseui/spinner';

export default function DataViewControl(props) {

    const CenterSpinner = withStyle(StyledSpinnerNext, {
        margin:"auto"
    });
    
    const { query } = props;

    let dataViewEvent = useQuery(query);
    
    if (dataViewEvent) {
        return (<pre>{JSON.stringify(dataViewEvent, undefined, 2)}</pre>);
    } else return (<CenterSpinner/>);
}
