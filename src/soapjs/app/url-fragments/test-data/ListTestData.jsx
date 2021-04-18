import {AggregateList, useEvent, useQuery, AggregateView} from "@soap/modules";
import React, {Fragment} from "react";


export function ListTestData() {

    const [e105, refresh] = useQuery({
        query: {
            $type: "Soap.Api.Sample.Messages.Commands.C111v2_GetRecentTestData"
        }
    });

    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E106v1_TestDataRemoved",
        onEventReceived(event, envelope) {
            refresh();
        }
    });

    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataUpserted",
        onEventReceived(event, envelope) {
            refresh();
        }
    });

    return (
        <Fragment>
            <AggregateList title="Recent Test Data" aggregates={e105?.e105_TestData} refreshFunction={() => refresh()}/>
            <AggregateView title="An Aggregate" aggregate={e105?.e105_TestData[0]} refreshFunction={() => refresh()} />
        </Fragment>);


    // <Cell><Button kind={KIND.minimal}
    //               onClick={() => location.href = "#/test-data/view/" + item.e105_Id}>View</Button></Cell>
    // <Cell><Button kind={KIND.minimal}
    //               onClick={() => location.href = "#/test-data/edit/" + item.e105_Id}>Edit</Button></Cell>

}
