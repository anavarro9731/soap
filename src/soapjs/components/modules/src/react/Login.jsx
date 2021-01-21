import React from "react";
import {useAuth0, withAuthenticationRequired} from "@auth0/auth0-react";
import {Route} from "react-router-dom";
import {useLogin} from "../hooks/useLogin";
import {Button, KIND, SIZE} from "baseui/button";
import DebugLayer from "./DebugLayer";
import config from '../soap/config'


export const Login = (props) => {
     
    const audience = config.vars.audience;

    const {
        isLoading,
        isAuthenticated,
        error,
        user,
        loginWithRedirect,
        logout
    } = useAuth0();

    const {
        idToken,
        accessToken,
        refresh
    } = useLogin();

    const [isOpen, setIsOpen] = React.useState(false);

    if (!config.auth0) {
        return null;
    }
    
    if (isLoading) {
        return <div>Loading...</div>;
    }
    if (error) {
        return <div>Oops... {error.message}</div>;
    }

    const urlParams = new URLSearchParams(window.location.search);
    const authDebug = urlParams.get('authDebug');
    if (authDebug && isOpen == false) {
        setIsOpen(true);
    }
    
    if (isAuthenticated) {

        return (<React.Fragment>
            <DebugLayer>
                <Button onClick={
                    () => {
                        console.log("accessToken", accessToken);
                        console.log("idToken", idToken);
                        console.log("user", user);
                        console.log("config", config);
                    }}>debugdata</Button>
                <Button onClick={() => refresh()}>refresh</Button>
            </DebugLayer>

            <div>Hello, {user.name}</div>
            &nbsp;
            <Button kind={KIND.secondary}
                    size={SIZE.compact} onClick={() => {
                config.auth0.isAuthenticated = false;
                config.auth0.accessToken = null;
                config.auth0.identityToken = null;
                logout({returnTo: window.location.origin});
            }}>Logout</Button>
        </React.Fragment>);
    } else {
        return <React.Fragment>
            <DebugLayer>
                <Button onClick={
                    () => {
                        console.log("config", config);
                    }}>debugdata</Button>
            </DebugLayer><Button kind={KIND.secondary}
                                 size={SIZE.compact} onClick={() => loginWithRedirect({
            redirect_uri: `${window.location.origin}`,
            audience: audience,
        })}>Log In</Button></React.Fragment>;
    }
};

export const ProtectedRoute = ({component, ...args}) => (
    <Route component={withAuthenticationRequired(component)} {...args} />
);

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
/*
export const history = createBrowserHistory();
export const onRedirectCallback = (appState) => {
    // Use the router's history module to replace the url
    history.replace(appState?.returnTo || window.location.pathname);
*/
    



