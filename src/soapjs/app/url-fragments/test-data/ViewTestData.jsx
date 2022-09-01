import {useQuery, CenterSpinner, AggregateView} from "@soap/modules";
import React from "react";
import {useParams} from "react-router-dom";

export function ViewTestData() {

    const {id} = useParams();

    const query = {
        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
        c110_TestDataId: id
    };

    const [data, refresh] = useQuery({query});

    if (data) {
        return (
            <AggregateView title="Test Data" aggregate={data} refresh={refresh}
                           propertyRenderer={{
                               "e102_Decimal": (value) => <>{"REPLACED"}</>
                           }}
                           hiddenFields={["e102_Guid"]}
            
            />
        );
    } else {
        return <CenterSpinner/>;
    }
}
