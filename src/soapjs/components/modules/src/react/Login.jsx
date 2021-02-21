import React, {useEffect} from "react";
import {useAuth0, withAuthenticationRequired} from "@auth0/auth0-react";
import {Route} from "react-router-dom";
import {useAuth} from "../hooks/useAuth";
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
        authReady,
        refresh
    } = useAuth();

    const urlParams = new URLSearchParams(window.location.search);
    const authDebug = urlParams.get('authDebug');
    
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
            </DebugLayer>) : null }
            
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
            { authDebug ? (<DebugLayer>
                <Button onClick={
                    () => {
                        console.log("config", config);
                    }}>debugdata</Button>
            </DebugLayer>) : null }
            
            <Button kind={KIND.secondary}
                                 size={SIZE.compact} onClick={() => loginWithRedirect({
            redirect_uri: `${window.location.origin}`,
            audience: audience,
        })}>Log In</Button></React.Fragment>;
    }
};

export const ProtectedRoute = ({component, ...args}) => {

    const { authReady, authEnabled } = useAuth();
    
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




    



