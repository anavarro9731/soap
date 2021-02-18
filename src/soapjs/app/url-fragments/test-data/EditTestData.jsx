import {AutoForm, JsonView, translate, useEvent} from "@soap/modules";
import {H1} from "baseui/typography";
import React, {useState} from "react";
import {Cell, Grid} from "baseui/layout-grid";
import wordKeys from "../../translations/word-keys";
import {useParams} from "react-router-dom";

export function EditTestData() {

    const [testDataId, setTestDataId] = useState();
    const [testDataUpdated, setTestDataUpdated] = useState(false);
    const { id } = useParams();

    
    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataUpserted",
        onEventReceived(event, envelope) {
            if (event.e104_TestDataId === testDataId) {
                setTestDataUpdated(true);
            }
        }
    });
    
    return (
        <Grid>
            <Cell span={6}>
                <H1>Form</H1>
                <AutoForm
                    query={{
                        $type: "C113v1_GetC107FormDataForEdit",
                        c113_TestDataId: id,
                        headers: []
                    }}
                    testFormHeader={translate(wordKeys.testFormHeader)}
                    afterSubmit={(command) => setTestDataId(command.c107_Guid)}
                    submitText="Save"
                    cancelText="Back"
                    cancelHref="#/test-data"/>
            </Cell>
            <Cell span={6}>
                <H1>View</H1>
                <JsonView query={{
                    $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
                    c110_TestDataId: testDataId
                }} sendQuery={testDataUpdated}/>
            </Cell>
        </Grid>
    );
}
