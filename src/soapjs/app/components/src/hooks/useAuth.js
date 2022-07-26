import {useCallback, useEffect, useState} from 'react';
import useGlobalState from "@vighnesh153/use-global-state";
import {useAuth0} from '@auth0/auth0-react';
import config from "../soap/config";
import {useIsConfigLoaded} from "./systemStateHooks";
import {optional, types, validateArgs} from "../soap/util";

export const useAuth = (callerName) => {

    /* i had these as 2 separate state vars, but when setting them in sequence i was getting mem leaks that I can't explain
    presumably due to a render in between. but why it seems to lose the ref to the owner at that exact moment every time
    is a mystery
     */
    const [tokens, setTokens] = useState(null);      
    const [ready, setReady] = useGlobalState(false);

    const {
        isLoading,
        isAuthenticated,
        getIdTokenClaims,
        getAccessTokenSilently,
        user,
        loginWithRedirect,
        getAccessTokenWithPopup
    } = useAuth0();

    const [refreshIndex, setRefreshIndex] = useState(0);
    const configLoaded = useIsConfigLoaded("UseAuth");
    const authEnabledInConfig = !!config.auth0;
    
    useEffect(() =>  {
        const t = new Date().getTime().toString();
        const x = callerName + t;
        
        (async () => {
            
            if (config.debugSystemState) 
                console.warn("Status at useAuth called by " + callerName + " at " + t +   
                " configLoaded:" + configLoaded,  
                "isLoading:" + isLoading, 
                "isAuthenticated:" + isAuthenticated,
                "authEnabledInConfig:" + authEnabledInConfig,
                "tokensSet:" + !!tokens, x);

            if (configLoaded) {
                if (authEnabledInConfig) {
                    if (!isLoading) {
                        if (isAuthenticated) {
                            const claims = await getIdTokenClaims();
                            // if you need the raw id_token, you can access it
                            // using the __raw property
                            const id_token = claims.__raw;
                            const access_token = await getAccessTokenSilently();
                            setTokensInConfig(id_token, access_token, user.sub);
                            setTokens({id_token, access_token});
                        }
                        //* if this is called and isAuthenticated is false, it would mean there was an error in Auth0 authenticating user
                        setReady (true);
                    }
                } else {
                    /* this would be where config is load but there is no auth0 object meaning auth is disabled, 
                    but for components waiting on a determination of whether auth is enabled or not, its now "ready" 
                    you could check for AuthEnabled and AuthReady separately but this makes it a bit 
                    easier and you can't forget to check one when there is only one variable.
                    maybe id should have just been called "ready" */
                    setReady(true);
                }
            }
        })();
        
        return function()  {
        }}
    , 
    [refreshIndex, isLoading, isAuthenticated, configLoaded]
    );

    const setTokensInConfig = useCallback((idToken, accessToken, username) => {
        validateArgs(
            [{idToken}, types.string],
            [{accessToken}, types.string],
            [{username}, types.string, optional]
        );
        config.auth0.isAuthenticated = true;
        config.auth0.accessToken = accessToken;
        config.auth0.identityToken = idToken;
        config.auth0.userName = username;
    },[]);
    
    const setTokensForceFully = useCallback((identityToken, accessToken, userName) => {
        setTokensInConfig(identityToken, accessToken, userName);
        setTokens({idToken, accessToken});
        setReady(true);
    },[]);
    
    return {
        idToken : tokens?.idToken,
        accessToken : tokens?.accessToken,
        authEnabled: authEnabledInConfig,
        authReady: ready,
        setTokensForceFully,
        requireAuth(onAuthenticated) {
            validateArgs([{onAuthenticated}, types.function]);
            if (ready) {
                if (authEnabledInConfig) {
                    if (isAuthenticated) {
                        onAuthenticated();
                    } else {
                        loginWithRedirect({appState: {returnTo: window.location.href}});
                    }
                } else { //* auth is disabled
                    onAuthenticated();
                }
            }
        }
        ,
        refresh() {
            setRefreshIndex(refreshIndex + 1);
        }
    };
}
