import React from 'react';
import {addTranslations, App, Login, ReactErrorBoundary, config} from '@soap/modules';
import {LightTheme} from 'baseui';
import {H2} from "baseui/typography";   
import ReactDOM from "react-dom";
import translations from "./translations/en-soap.app.sample-default";
import {ALIGN, HeaderNavigation, StyledNavigationItem, StyledNavigationList} from "baseui/header-navigation";
import {StyledLink} from "baseui/link";
import {Route, Switch} from "react-router-dom";
import TestData from './url-fragments/test-data/Index'

addTranslations(translations);
config.debugSystemState = false;
config.disableGeneralLogger();
globalThis.Soap.forceConsoleLogging = true;
function Index() {
    return (
        
        <ReactErrorBoundary>
        <App theme={LightTheme}>
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
                            <StyledLink href="#/test-data">
                                Test Data
                            </StyledLink>
                        </StyledNavigationItem>
                    </StyledNavigationList>
                    <StyledNavigationList $align={ALIGN.right}>
                        <Login />
                    </StyledNavigationList>
                </HeaderNavigation>
                <Switch>
                    <Route path="/test-data" component={TestData} />
                    <Route path="/">
                        <Home/>
                    </Route>
                </Switch>
        </App>
        </ReactErrorBoundary>
        
    );
}

ReactDOM.render(<Index/>, document.getElementById('content'));

function Home() {
    return (
        <div>
            <H2>Hello World, I'm Home!</H2>
        </div>
    );
}
