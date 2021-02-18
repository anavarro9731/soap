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
                        {getContent()}
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
                    redirectUri={window.location.origin}
                    useRefreshTokens={true}
                >
                    {props.children}
                </Auth0Provider>);
            } else {
                {props.children}
            }
        } else {
            return (<CenterSpinner />);
        }
    }
}

