import {AutoForm, JsonView, translate, useEvent} from "@soap/modules";
import {H1} from "baseui/typography";
import React, {useState} from "react";
import {Cell, Grid} from "baseui/layout-grid";
import wordKeys from "../../translations/word-keys";
import { useLocation } from 'react-router-dom';

export function CreateTestData() {

    const [testDataId, setTestDataId] = useState();
    const [testDataCreated, setTestDataCreated] = useState(false);
    const location = useLocation();
    
    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataUpserted",
        onEventReceived(event, envelope) {
            if (event.e104_TestDataId === testDataId) {
                setTestDataCreated(true);
            }
        }
    });
    
    return (
        <Grid>
            <Cell span={6}>
                <H1>Form</H1>
                <AutoForm
                    query={{
                        $type: "Soap.Api.Sample.Messages.Commands.C109v1_GetC107DefaultFormData"
                    }}
                    testFormHeader={translate(wordKeys.testFormHeader)}
                    afterSubmit={(command) => setTestDataId(command.c107_Guid)}
                    submitText="Save"
                    cancelText="Back"
                    afterCancel={() => location.href="#/test-data"}
                />
            </Cell>
            <Cell span={6}>
                <H1>View</H1>
                <JsonView query={{
                    $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
                    c110_TestDataId: testDataId
                }} sendQuery={testDataCreated}/>
            </Cell>
        </Grid>
    );
}
