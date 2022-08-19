import {AggregateList, AggregateView, EntityMenu, getIdOfMessageEntity, useEvent, useQuery} from "@soap/modules";
import React, {Fragment, useCallback} from "react";
import {CreateTestData} from "./drawers/CreateTestData";
import {EditTestData} from "./drawers/EditTestData";
import {RemoveTestData} from "./modals/RemoveTestData";
import {StatefulPopover} from "baseui/popover";
import {Button, SIZE} from "baseui/button";

export function ListTestData() {

    const [e105, refresh] = useQuery({
        query: {
            $type: "Soap.Api.Sample.Messages.Commands.C111v2_GetRecentTestData"
        }
    });

    const receiver = useCallback(() => {
        refresh();
    }, []);

    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E106v1_TestDataRemoved",
        onEventReceived: receiver
    });

    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataUpserted",
        onEventReceived: receiver
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
                        (entity) => <RemoveTestData entity={{id: entity.e105_Id, label: entity.e105_Label}}/>
                    ]),
                    "e105_CChild": new EntityMenu((entity) => alert(entity), []),
                    "e105_CChildren-ArrayItems": new EntityMenu([(entity) => <EditTestData/>, (entity) =>
                        <EditTestData/>], [
                        (entity) => <button onClick={() => alert(JSON.stringify(entity))}>Show JSON</button>,
                    ])
                }
            }
                           propertyRenderer={{
                               "e105_CChild": (value) => <>
                                   <StatefulPopover
                                       content={() => (
                                           <AggregateView title={"test"} aggregate={value}/>
                                       )}
                                       returnFocus
                                       autoFocus
                                   >
                                       <Button size={SIZE.compact}>Pbis</Button>
                                   </StatefulPopover>

                               </>
                           }}
                           expandedFields={[]}
                           hiddenFields={[]}
                           title="Recent Test Data" aggregates={e105?.e105_TestData} refreshFunction={() => refresh()}
            />

            {/*<AggregateView title="An Aggregate" aggregate={e105?.e105_TestData[0]} refreshFunction={() => refresh()}/>*/}
        </Fragment>);
}
