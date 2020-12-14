import React from 'react';
import {useQuery} from '@soap/modules';

export default function DataViewControl(props) {
    
    let dataViewEvent = useQuery(props.query);
    
    if (!dataViewEvent) {
        return (<pre>{JSON.stringify(dataViewEvent, undefined, 2)}</pre>);
    }
}
