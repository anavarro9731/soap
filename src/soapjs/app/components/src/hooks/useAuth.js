import {useCallback, useEffect, useState} from 'react';
import {useAuth0} from '@auth0/auth0-react';
import useGlobalState from "@vighnesh153/use-global-state";
import config from "../soap/config";
import {useIsConfigLoaded} from "./systemStateHooks";
import {optional, types, validateArgs} from "../soap/util";

export const useAuth = (callerName) => {
    
    const [tokens, setTokens] = useGlobalState(null);      
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
                "tokens", tokens, x);

            if (configLoaded && !ready) {  /* because we are using global state, "ready" can be set by other components,
            and we wouldn't pick it up, that would mean that when our component runs it might proceed to setstate such as ready again,
            and that would then cause errors on the other components that are unmounted but were listening to the global state! 
            or at least thats my assumption, but when i switched to global state i started getting a lot of mem leak errors,
            and adding !ready solved that. !ready should really be checked anyway so it makes good sense. */  
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
        
        return () =>  {
        }},  [refreshIndex, isLoading, isAuthenticated, configLoaded, ready, tokens]
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
        setTokens({id_token: identityToken, access_token: accessToken});
        setReady(true);
    },[]);
    
    return {
        idToken : tokens?.id_token,
        accessToken : tokens?.access_token,
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
