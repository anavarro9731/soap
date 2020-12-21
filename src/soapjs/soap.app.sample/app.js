import React, {useState} from 'react';
import ReactDOM from 'react-dom';
import {addTranslations, useEvent, bus, getHeader, headerKeys } from '@soap/modules';
import translations from "./translations/en-soap.app.sample-default";
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LightTheme, LocaleProvider} from 'baseui';
import SoapFormControl from "./FormControl";
import DataViewControl from "./DataViewControl";
import {Cell, Grid} from 'baseui/layout-grid';
import {H1} from "baseui/typography";
import {PLACEMENT, toaster, ToasterContainer} from 'baseui/toast';
import {SnackbarProvider, DURATION,} from 'baseui/snackbar';

const localeOverride = {
    fileuploader: {
        dropFilesToUpload: "Drop a file here, or ",
        browseFiles: "Browse for a file"
    }
};
const engine = new Styletron();

addTranslations(translations);
//config.logClassDeclarations = true;
//config.logFormDetail = true;

function App() {
    
    const [testDataId, setTestDataId] = useState();
    const [testDataCreated, setTestDataCreated] = useState(false);
    
    useEvent({ 
        eventName: "Soap.Api.Sample.Messages.Events.E104v1_TestDataAdded", 
        onEventReceived(event, envelope) {
            if (event.e104_TestDataId === testDataId) {;
            const y = event.asdasdasda;
            event.e104_TestDataId = "dfsdfsd";
            event.asdasdasd= "dsdsdasd";
            setTestDataCreated(true);    
            }
        }
    });

    useEvent({
        eventName: "Soap.Interfaces.Messages.E001v1_MessageFailed",
        onEventReceived(event, envelope) {
                toaster.negative(event.e001_ErrorMessage);
        }
    });
    
    function renderDataView(testDataId) {
      return testDataCreated ? (<DataViewControl query={{
          $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestData, Soap.Api.Sample.Messages',
          c110_TestDataId: testDataId,
          headers: []
      }}/>) : null;  
    } 
    
    return (
        <LocaleProvider locale={localeOverride}>
            <StyletronProvider value={engine}>
                <BaseProvider theme={LightTheme}>
                    <ToasterContainer  autoHideDuration={4000} placement={PLACEMENT.topRight}>
                        <SnackbarProvider defaultDuration={DURATION.medium} >
                    <Grid>
                        <Cell span={6}>
                            <H1>Form</H1>
                            <SoapFormControl formEventName="E103v1_GetC107Form" afterSubmit={(command) => setTestDataId(command.c107_Guid)} />
                        </Cell>
                        <Cell span={6}>
                            <H1>View</H1>
                            {renderDataView(testDataId)}
                        </Cell>
                    </Grid>
                        </SnackbarProvider>
                    </ToasterContainer>
                </BaseProvider>
            </StyletronProvider>
        </LocaleProvider>);
}

ReactDOM.render(<App/>, document.getElementById('content'));
