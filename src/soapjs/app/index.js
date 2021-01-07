import React, {useState} from 'react';
import {addTranslations, App, AutoForm, JsonView, translate, useEvent, useQuery} from '@soap/modules';
import {Cell, Grid} from 'baseui/layout-grid';
import {LightTheme} from 'baseui';
import {H1,H2,H3,H4,H5} from "baseui/typography";
import ReactDOM from "react-dom";
import translations from "./translations/en-soap.app.sample-default";
import wordKeys from './translations/word-keys';
import {HashRouter as Router, Link, Route, Switch} from "react-router-dom";
import {
    HeaderNavigation,
    ALIGN,
    StyledNavigationList,
    StyledNavigationItem
} from "baseui/header-navigation";
import { StyledLink } from "baseui/link";
import { ListItem, ListItemLabel } from "baseui/list";

//config.logClassDeclarations = true;
//config.logFormDetail = true;
addTranslations(translations);

function Index() {
    return (
        <App theme={LightTheme}>
            <Router>
                <HeaderNavigation>
                    <StyledNavigationList $align={ALIGN.left}>
                        <StyledNavigationItem>Test App</StyledNavigationItem>
                    </StyledNavigationList>
                    <StyledNavigationList $align={ALIGN.center}>
                        <StyledNavigationItem>
                            <StyledLink href="#/">
                                Home
                            </StyledLink>   
                        </StyledNavigationItem>
                        <StyledNavigationItem>
                            <StyledLink href="#/TestData">
                                Add Test Data Item
                            </StyledLink>
                        </StyledNavigationItem>
                    </StyledNavigationList>
                </HeaderNavigation>
                <Switch>
                    <Route path="/testdata">
                        <CreateTestData/>
                    </Route>
                    <Route path="/">
                        <Home/>
                    </Route>
                </Switch>
            </Router>
        </App>
    );
}

ReactDOM.render(<Index/>, document.getElementById('content'));

function Home() {
    return (
        <div>
            <H2>Hello World, I'm Home!</H2>
            <RecentTestItems />
        </div>  
    );
    
}

function RecentTestItems() {
    
    let listitems;

    let items = useQuery({query:{
            $type: "GETFORMITEMSCMD",
            headers: []
        }});

    if (items) {
        
    }
    return (
        <div>
            <H5>Recently Added Test Items</H5>
            <ul>
                {listitems}            
            </ul>
        </div>
    );
}

function CreateTestData() {
    
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
                    $type: 'Soap.Api.Sample.Messages.Commands.C110v1_GetTestData, Soap.Api.Sample.Messages',
                    c110_TestDataId: testDataId,
                    headers: []
                }} sendQuery={testDataCreated}/>
            </Cell>
        </Grid>
    );
}
