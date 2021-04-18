import React from 'react';
import {useQuery} from '../hooks/useQuery';
import {withStyle} from 'baseui';
import {StyledSpinnerNext} from 'baseui/spinner';

export function JsonView(props) {

    const CenterSpinner = withStyle(StyledSpinnerNext, {
        margin: "auto"
    });

    const {query, sendQuery} = props;

    const [dataViewEvent, refresh] = useQuery({query, sendQuery});

    if (dataViewEvent) {
        return (<pre>{JSON.stringify(dataViewEvent, undefined, 2)}</pre>);
    } else if (sendQuery) {
        return (<CenterSpinner/>);
    } else {
        return null;
    }
}
