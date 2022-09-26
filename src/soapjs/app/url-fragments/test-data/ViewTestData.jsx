import {useQuery, CenterSpinner, AggregateView} from "@soap/modules";
import React from "react";
import {useParams} from "react-router-dom";
import {RemoveTestData} from "./modals/RemoveTestData";
import {EntityMenu} from "../../components/src";

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
                           entityMenus={
                               {
                                   "root": new EntityMenu(null,
                                   [
                                       (entity) => {
                                        return (<RemoveTestData afterSubmitHref="#/test-data" entity={{id: entity.e102_TestData.e102_Guid, label: entity.e102_TestData.e102_String}} />)
                                       }
                                   ])
                                   
                               }}
                           
                           hiddenFields={["e102_Guid"]}
                           refreshFunction={() => refresh()}
                           backFunction={() => location.href = "#/test-data/"}
            
            />
        );
    } else {
        return <CenterSpinner/>;
    }
}
