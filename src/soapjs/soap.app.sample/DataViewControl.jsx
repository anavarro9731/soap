import React from 'react';
import {useQuery} from '@soap/modules';

export default function DataViewControl(props) {
    
    const { query } = props;

    let dataViewEvent = useQuery(query);
    
    if (dataViewEvent) {
        return (<pre>{JSON.stringify(dataViewEvent, undefined, 2)}</pre>);
    } else return null;
}
