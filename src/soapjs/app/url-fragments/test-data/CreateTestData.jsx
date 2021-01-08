import {AutoForm, JsonView, translate, useEvent} from "@soap/modules";
import {H1} from "baseui/typography";
import React, {Fragment, useState} from "react";
import {Cell, Grid} from "baseui/layout-grid";
import wordKeys from "../../translations/word-keys";
import {Route} from "react-router-dom";

export function CreateTestData() {

    const [testDataId, setTestDataId] = useState();
    const [testDataCreated, setTestDataCreated] = useState(false);

    useEvent({
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataAdded",
        onEventReceived(event, envelope) {
            if (event.e104_TestDataId === testDataId) {
                setTestDataCreated(true);
            }
        }
    });

    return (
        <Fragment>
            <Grid>
                <Cell span={6}>
                    <H1>Form</H1>
                    <AutoForm formEventName="Soap.Api.Sample.Messages.Events.E103v1_GetC107Form"
                              testFormHeader={translate(wordKeys.testFormHeader)}
                              afterSubmit={(command) => setTestDataId(command.c107_Guid)}/>
                </Cell>
                <Cell span={6}>
                    <H1>View</H1>
                    <JsonView query={{
                        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestDataById',
                        c110_TestDataId: testDataId
                    }} sendQuery={testDataCreated}/>
                </Cell>
            </Grid>
        </Fragment>
    );
}
