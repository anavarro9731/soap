import {useQuery, CenterSpinner, AggregateView} from "@soap/modules";
import React from "react";
import {useParams} from "react-router-dom";
import {AggregateList} from "../../components/src";

export function ViewTestData() {

    const {id} = useParams();

    const query = {
        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
        c110_TestDataId: id
    };

    const [data, refresh] = useQuery({query});

    if (data) {
        return (
            <AggregateList title="Test Data" aggregates={[data]} refresh={refresh}/>
        );
    } else {
        return <CenterSpinner/>;
    }
}
