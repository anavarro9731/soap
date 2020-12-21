import React from 'react';
import {useEvent} from '../hooks/useEvent';
import {Client as Styletron} from 'styletron-engine-atomic';
import {Provider as StyletronProvider} from 'styletron-react';
import {BaseProvider, LocaleProvider} from 'baseui';
import {PLACEMENT, toaster, ToasterContainer} from 'baseui/toast';
import {DURATION, SnackbarProvider,} from 'baseui/snackbar';

const localeOverride = {
    fileuploader: {
        dropFilesToUpload: "Drop a file here, or ",
        browseFiles: "Browse for a file"
    }
};

const engine = new Styletron();

export default function App(props) {

    useEvent({
        eventName: "Soap.Interfaces.Messages.E001v1_MessageFailed",
        onEventReceived(event, envelope) {
            toaster.negative(event.e001_ErrorMessage);
        }
    });

    const override = !!props.localeOverride ? {...localeOverride, ...props.localOverride} : localeOverride;

    return (
        <LocaleProvider locale={override}>
            <StyletronProvider value={engine}>
                <BaseProvider theme={props.theme}>
                    <ToasterContainer autoHideDuration={4000} placement={PLACEMENT.topRight}>
                        <SnackbarProvider defaultDuration={DURATION.medium}>
                            {props.children}
                        </SnackbarProvider>
                    </ToasterContainer>
                </BaseProvider>
            </StyletronProvider>
        </LocaleProvider>);
}

