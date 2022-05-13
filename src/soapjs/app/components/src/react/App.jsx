import React, {useEffect} from 'react';
import {useEvent} from '../hooks/useEvent';
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LocaleProvider} from 'baseui';
import {PLACEMENT as PLACEMENT_TOASTER, toaster, ToasterContainer} from 'baseui/toast';
import {DURATION, PLACEMENT as PLACEMENT_SNACKBAR, SnackbarProvider} from 'baseui/snackbar';
import {Auth0Provider} from "@auth0/auth0-react";
import config from "../soap/config";
import {useIsConfigLoaded} from "../hooks/systemStateHooks";
import {getHeader} from "../soap/util";
import {headerKeys} from "../soap/messages";
import {HashRouter as Router} from "react-router-dom";
import {CenterSpinner} from "./CenterSpinner";

const localeOverride = {
    fileuploader: {
        dropFilesToUpload: "Drop a file here, or ",
        browseFiles: "Browse for a file"
    }
};

const engine = new Styletron();

export function App(props) {

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
            const errorId = !!props.showErrorId ? "Error Id: " + getHeader(event, headerKeys.messageId) : "";
            const error = event.e001_ErrorMessage + " " + errorId;
            toaster.negative(error);
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
                <ToasterContainer autoHideDuration={8000} placement={PLACEMENT_TOASTER.topRight}>
                    <SnackbarProvider defaultDuration={DURATION.short} placement={PLACEMENT_SNACKBAR.bottom}>
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
                    redirectUri={config.auth0.redirectUri}
                    onRedirectCallback={onRedirectCallback}
                    useRefreshTokens={true}
                >
                    {props.children}
                </Auth0Provider>);
            } else {
                return (<React.Fragment>{props.children}</React.Fragment>);
            }
        } else {
            return (<CenterSpinner/>);
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
