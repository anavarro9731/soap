import React from 'react';
import {App, Login, ReactErrorBoundary, config} from '@soap/modules';
import {LightTheme} from 'baseui';
import {H2} from "baseui/typography";   
import ReactDOM from "react-dom";
import {ALIGN, HeaderNavigation, StyledNavigationItem, StyledNavigationList} from "baseui/header-navigation";
import {StyledLink} from "baseui/link";
import {Route, Switch} from "react-router-dom";

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
                    </StyledNavigationList>
                    <StyledNavigationList $align={ALIGN.right}>
                        <Login />
                    </StyledNavigationList>
                </HeaderNavigation>
                <Switch>
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
