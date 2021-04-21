import {AggregateList, AggregateView, useEvent, useQuery} from "@soap/modules";
import React, {Fragment} from "react";
import {CreateTestData} from "./drawers/CreateTestData";
import {EntityMenu} from "../../../components/modules/src/react/Tables";
import {EditTestData} from "./drawers/EditTestData";
import {getIdOfMessageEntity} from "../../../components/modules/src/soap/messages";

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
                    "root-ArrayItems": new EntityMenu(null, [
                        (entity) => <EditTestData id={getIdOfMessageEntity(entity)}/>
                    ]),
                    "e105_Child": new EntityMenu((entity) => alert(entity), [
                        (entity) => <EditTestData id={getIdOfMessageEntity(entity)}/>
                    ]),
                    "e105_Children2": new EntityMenu((entity) => alert(entity), [
                        (entity) => <button onClick={() => alert(JSON.stringify(entity))}>1</button>,
                        (entity) => <button onClick={() => alert(getIdOfMessageEntity(entity))}>2</button>
                    ]),
                    "e105_Children-ArrayItems": new EntityMenu((entity) => alert(entity), [
                        (entity) => <button onClick={() => alert(JSON.stringify(entity))}>1</button>,
                        (entity) => <button onClick={() => alert(getIdOfMessageEntity(entity))}>2</button>
                    ])
                }
            } title="Recent Test Data" aggregates={e105?.e105_TestData} refreshFunction={() => refresh()}/>

            <AggregateView title="An Aggregate" aggregate={e105?.e105_TestData[0]} refreshFunction={() => refresh()}/>
        </Fragment>);
}
