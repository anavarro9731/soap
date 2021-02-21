import React from 'react';
import {useEvent} from '../hooks/useEvent';
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LocaleProvider, withStyle} from 'baseui';
import {PLACEMENT, toaster, ToasterContainer} from 'baseui/toast';
import {DURATION, SnackbarProvider,} from 'baseui/snackbar';
import {Auth0Provider } from "@auth0/auth0-react";
import config from "../soap/config";
import {useEffect, useReducer} from "react";
import {useIsConfigLoaded} from "../hooks/systemStateHooks";
import {StyledSpinnerNext} from "baseui/spinner";
import {getHeader} from "../soap/util";
import {headerKeys} from "../soap/messages";
import {HashRouter as Router, Route, Switch} from "react-router-dom";

const localeOverride = {
    fileuploader: {
        dropFilesToUpload: "Drop a file here, or ",
        browseFiles: "Browse for a file"
    }
};

const engine = new Styletron();

const CenterSpinner = withStyle(StyledSpinnerNext, {
    margin: "auto"
})

export default function App(props) {
    
    function onRedirectCallback(appState) {
        if (config.debugSystemState) {
            console.warn("redirect callback ran", appState, history);    
        }
        if (!!appState?.returnTo) {
            window.location.href = appState?.returnTo    
        }
    };
    
    useEvent({
        eventName: "Soap.Interfaces.Messages.E001v1_MessageFailed",
        onEventReceived(event, envelope) {
            toaster.negative(`${event.e001_ErrorMessage} Error Id: ${getHeader(event, headerKeys.messageId)}`);
        }
    });
    
    const configLoaded = useIsConfigLoaded("app.jsx");
    const override = !!props.localeOverride ? {...localeOverride, ...props.localOverride} : localeOverride;

    useEffect(() => {
        if (config.debugSystemState) console.warn("app.jsx rendered");
    });
    
    return (<LocaleProvider locale={override}>
        <StyletronProvider value={engine}>
            <BaseProvider theme={props.theme}>
                <ToasterContainer autoHideDuration={4000} placement={PLACEMENT.topRight}>
                    <SnackbarProvider defaultDuration={DURATION.short}>
                        <Router>
                        {getContent()}
                        </Router>
                    </SnackbarProvider>
                </ToasterContainer>
            </BaseProvider>
        </StyletronProvider>
    </LocaleProvider>);

    function getContent() {

        if (configLoaded) {
            if (config.auth0) {
                return (<Auth0Provider
                    domain={config.auth0.tenantDomain}
                    clientId={config.auth0.uiAppId}
                    audience={config.vars.audience}
                    redirectUri={"http://localhost:1234/"}
                    onRedirectCallback={onRedirectCallback}
                    useRefreshTokens={true}
                >
                    {props.children}
                </Auth0Provider>);
            } else {
                return (<React.Fragment>{props.children}</React.Fragment>);
            }
        } else {
            return (<CenterSpinner />);
        }
    }
}


/* this code would be useful 
after login for changing where the back button goes but HashRouter doesn't 
support history object and using the Azure CDN which would allow us to
get away from HashRouter and Use Router would be a problem because its
not available for test and the delay in publishing would make dev impossible
It's not that important a feature to spend any more time on it but worth
noting for future options as this is how the sample code works.

to use it you would import these two constants into the main page
and set the onRedirectCallback={onRedirectCallback} attr of the Auth0provider
and history={history} attr of the Router object and
import {createBrowserHistory} from "history";
 */
