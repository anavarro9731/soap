import {AggregateList, AggregateView, useEvent, useQuery, EntityMenu, getIdOfMessageEntity} from "@soap/modules";
import React, {Fragment} from "react";
import {CreateTestData} from "./drawers/CreateTestData";
import {EditTestData} from "./drawers/EditTestData";
import {RemoveTestData} from "./modals/RemoveTestData";

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
            <AggregateList entityMenus={
                {
                    "root": new EntityMenu(null, [
                        () => <CreateTestData/>
                    ]),
                    "root-ArrayItems": new EntityMenu(entity => location.href = "#/test-data/view/" + entity.e105_Id, [
                        (entity) => <EditTestData id={getIdOfMessageEntity(entity)}/>,
                        (entity) => <RemoveTestData entity={{id: entity.e105_Id , label: entity.e105_Label}}/>
                    ]),
                    "e105_CChild": new EntityMenu((entity) => alert(entity), [
                        
                    ]),
                    "e105_CChildren-ArrayItems": new EntityMenu((entity) => alert(entity), [
                        (entity) => <button onClick={() => alert(JSON.stringify(entity))}>Show JSON</button>,
                    ])
                }
            }
                           expandedFields={["e105_BChildren"]}
                           hiddenFields={["e105_BChildId", "e105_BChildLong"]}
                           title="Recent Test Data" aggregates={e105?.e105_TestData} refreshFunction={() => refresh()}/>

            <AggregateView title="An Aggregate" aggregate={e105?.e105_TestData[0]} refreshFunction={() => refresh()}/>
        </Fragment>);
}
