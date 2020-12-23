import React, {useState} from 'react';
import {addTranslations, translate, useEvent, AutoForm, JsonView, App, config} from '@soap/modules';
import {Cell, Grid} from 'baseui/layout-grid';
import {LightTheme} from 'baseui';
import {H1} from "baseui/typography";
import ReactDOM from "react-dom";
import translations from "./translations/en-soap.app.sample-default";
import wordKeys from './translations/word-keys';
import * as signalR from '@microsoft/signalr';


//config.logClassDeclarations = true;
//config.logFormDetail = true;
addTranslations(translations);

config.receiver = async (processor) => {

    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:7071/api')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Debug)
        .build();

    hubConnection.on('eventReceived', async message => {
        const messageObj = JSON.parse(message.substring(3)); //don't let signalr do the serialising or it will use the wrong JSON settings
        await processor(messageObj);
    });

    hubConnection
        .start()
        .then(function () {
            onConnected();
        })
        .catch(function (error) {
            console.error("Error getting signalr message************:" +error.message);
        });
    
    function onConnected() {
        console.warn("onConnected called");
    }
}
function Index() {

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
        <App theme={LightTheme}>
            <Grid>
                <Cell span={6}>
                    <H1>Form</H1>
                    <AutoForm formEventName="E103v1_GetC107Form" testFormHeader={translate(wordKeys.testFormHeader)}
                                     afterSubmit={(command) => setTestDataId(command.c107_Guid)} sendQuery={true}/>
                </Cell>
                <Cell span={6}>
                    <H1>View</H1>
                    <JsonView query={{
                        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestData, Soap.Api.Sample.Messages',
                        c110_TestDataId: testDataId,
                        headers: []
                    }} sendQuery={testDataCreated}/>
                </Cell>
            </Grid>
        </App>
    );

}

ReactDOM.render(<Index/>, document.getElementById('content'));
