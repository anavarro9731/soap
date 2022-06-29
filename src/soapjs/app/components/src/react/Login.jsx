import React, {useEffect, useState} from "react";
import {useAuth0, withAuthenticationRequired} from "@auth0/auth0-react";
import {Route} from "react-router-dom";
import {useAuth} from "../hooks/useAuth";
import {Button, KIND, SIZE} from "baseui/button";
import DebugLayer from "./DebugLayer";
import config from '../soap/config'

export const Login = (props) => {

    
    const audience = config.auth0.audience;

    const {
        isLoading,
        isAuthenticated,
        error,
        user,
        loginWithRedirect,
        buildAuthorizeUrl,
        logout
    } = useAuth0();

    const {
        idToken,
        accessToken,
        authReady,
        refresh
    } = useAuth();

    const urlParams = new URLSearchParams(window.location.search);
    const authDebug = urlParams.get('authDebug');
    const code = urlParams.get('code');
    
    const [signupUrl, setSignupUrl] = useState();

    useEffect(() => {
        const getCustomAuthoriseUrl = async () => {
            if (authReady && !!config.auth0) {
                if (config.showSignup) {
                    let authoriseUrl = await buildAuthorizeUrl();
                    authoriseUrl += '&screen_hint=signup';
                    setSignupUrl(authoriseUrl);
                }
            }
        }
        getCustomAuthoriseUrl();
        
        //* remove code param from url to prevent entries in browser history causing failure behaviour (e.g. endless redirects, invalid state errors)
        //* the timing 
        if (!!code) {
            setTimeout(() => {
                window.history.replaceState({}, document.title, location.origin + '/#/');
            }, 500); //*ugly hack but you need to ensure that auth0 library has fully processed the code, without this is doesn't pickup the login
            
        }
    }, [authReady], [code]);

    if (!authReady || !config.auth0) {
        return null;
    }

    if (isLoading) {
        return <div>Loading...</div>;
    }
    if (error) {
        return <div>Oops... {error.message}</div>;
    }

    if (isAuthenticated) {

        return (<React.Fragment>

            {authDebug ?
                (<DebugLayer>
                    <Button onClick={
                        () => {
                            console.log("accessToken", accessToken);
                            console.log("idToken", idToken);
                            console.log("user", user);
                            console.log("config", config);
                        }}>debugdata</Button>
                    <Button onClick={() => refresh()}>refresh</Button>
                </DebugLayer>) : null}

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
            {authDebug ? (<DebugLayer>
                <Button onClick={
                    () => {
                        console.log("config", config);
                    }}>debugdata</Button>
            </DebugLayer>) : null}
            {signupUrl ? <Button kind={KIND.secondary}
                                 size={SIZE.compact} onClick={() => window.location.href = signupUrl}>Sign
                Up</Button> : null}
            <Button kind={KIND.secondary}
                    size={SIZE.compact} onClick={() => loginWithRedirect({
                redirect_uri: `${window.location.origin}`,
                audience: audience,
            })}>Log In</Button></React.Fragment>;
    }
};

export const ProtectedRoute = ({component, ...args}) => {

    const {authReady, authEnabled} = useAuth();

    if (authReady) {
        if (authEnabled) {
            return (<Route component={withAuthenticationRequired(component, {
                returnTo: window.location.href
            })} {...args} />);
        } else {
            return (<Route component={component} {...args} />);
        }
    } else {
        return null;
    }

};




    



