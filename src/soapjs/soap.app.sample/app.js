import React, {useState} from 'react';
import ReactDOM from 'react-dom';
import {addTranslations, useEvent, bus } from '@soap/modules';
import translations from "./translations/en-soap.app.sample-default";
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LightTheme, LocaleProvider} from 'baseui';
import SoapFormControl from "./FormControl";
import DataViewControl from "./DataViewControl";
import {Cell, Grid} from 'baseui/layout-grid';
import {H1} from "baseui/typography";

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
    
    const [testDataId, setTestDataId] = useState(null);
    
    // useEvent("test-data-submitted", (data, env) => {
    //     setTestDataId(data);
    // });
//afterSubmit={(command) => bus.publish(bus.channels.events, "test-data-submitted", command.c107_Guid)
    
    function renderDataView(testDataId) {
      return !!testDataId ? (<DataViewControl query={{
          $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestData, Soap.Api.Sample.Messages',
          c110_TestDataId: testDataId,
          headers: []
      }}/>) : null;  
    } 
    
    return (
        <LocaleProvider locale={localeOverride}>
            <StyletronProvider value={engine}>
                <BaseProvider theme={LightTheme}>
                    <Grid>
                        <Cell span={6}>
                            <H1>Form</H1>
                            <SoapFormControl formEventName="E103v1_GetC107Form" afterSubmit={(command) => { console.log("a",command.c107_Guid); setTestDataId(command.c107_Guid); }}/>
                        </Cell>
                        <Cell span={6}>
                            <H1>View</H1>
                            {renderDataView(testDataId)}
                        </Cell>
                    </Grid>
                </BaseProvider>
            </StyletronProvider>
        </LocaleProvider>);
}

ReactDOM.render(<App/>, document.getElementById('content'));
