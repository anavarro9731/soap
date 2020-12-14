import React from 'react';
import ReactDOM from 'react-dom';
import {addTranslations, config } from '@soap/modules';
import translations from "./translations/en-soap.app.sample-default";
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LightTheme} from 'baseui';
import SoapFormControl from "./FormControl";
import DataViewControl from "./DataViewControl";
import {LocaleProvider} from 'baseui'

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
    return (
        <LocaleProvider locale={localeOverride}>
        <StyletronProvider value={engine}>
            <BaseProvider theme={LightTheme}>
                <div>
                    <DataViewControl query={{
                        $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestData, Soap.Api.Sample.Messages',
                        c110_TestDataId:"597961fb-b86a-420f-bd7e-2226081293c9",
                        headers: []
                    }} />
                    <SoapFormControl formEventName="E500v1_GetC107Form"/>    
                </div>
                
            </BaseProvider>
        </StyletronProvider>
        </LocaleProvider>);
}

ReactDOM.render(<App/>, document.getElementById('content'));
