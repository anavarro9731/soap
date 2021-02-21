import {useEffect, useState} from 'react';
import {useAuth0} from '@auth0/auth0-react';
import config from "../soap/config";
import {useIsConfigLoaded} from "./systemStateHooks";
import {validateArgs, types} from "../soap/util";

export const useAuth = () => {

    const [idToken, setIdToken] = useState(null);
    const [accessToken, setAccessToken] = useState(null);
    const [authReady, setAuthReady] = useState(false);

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
    const configLoaded = useIsConfigLoaded("useAuth.js");

    useEffect(() => {
        (async () => {

            if (config.debugSystemState) console.warn("values at useAuth's useEffectHook", "configLoaded:" + configLoaded, "isLoading:" + isLoading, "isAuthenticated:" + isAuthenticated);

            if (configLoaded) {
                if (config.auth0) {
                    if (!isLoading) {
                        if (isAuthenticated) {
                            const claims = await getIdTokenClaims();
                            // if you need the raw id_token, you can access it
                            // using the __raw property
                            const id_token = claims.__raw;
                            setIdToken(id_token);

                            const access_token = await getAccessTokenSilently();
                            setAccessToken(access_token);

                            config.auth0.isAuthenticated = true;
                            config.auth0.accessToken = access_token;
                            config.auth0.identityToken = id_token;
                            config.auth0.userName = user.sub;
                        }
                        setAuthReady(true);
                    }
                } else {
                    setAuthReady(true);
                }
            }
        })();
    }, [refreshIndex, isLoading, isAuthenticated, configLoaded]);

    return {
        idToken,
        accessToken,
        authEnabled: !!config.auth0,
        authReady,
        requireAuth(onAuthenticated) {
            validateArgs([{onAuthenticated}, types.function]);
            if (authReady && !!config.auth0) {
                if (isAuthenticated) {
                    onAuthenticated();
                } else {
                    loginWithRedirect({appState: {returnTo: window.location.href}});
                }    
            }
        },
        refresh() {
            setRefreshIndex(refreshIndex + 1)
        }
    };
}
