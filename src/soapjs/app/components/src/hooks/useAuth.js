import {useEffect, useState} from 'react';
import {useAuth0} from '@auth0/auth0-react';
import config from "../soap/config";
import {useIsConfigLoaded} from "./systemStateHooks";
import {optional, types, validateArgs} from "../soap/util";

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

            if (config.debugSystemState) console.warn("values at useAuth's useEffectHook", "configLoaded:" + configLoaded,  "isLoading:" + isLoading, "isAuthenticated:" + isAuthenticated);

            if (configLoaded) {
                if (config.auth0) {
                    if (!isLoading) {
                        if (isAuthenticated) {
                            const claims = await getIdTokenClaims();
                            // if you need the raw id_token, you can access it
                            // using the __raw property
                            const id_token = claims.__raw;
                            const access_token = await getAccessTokenSilently();
                            setAuthenticated(id_token, access_token, user.sub);
                        }
                        setAuthReady(true);
                    }
                } else {
                    /* this would be where config is load but not auth0 object, so auth is disabled, but for components waiting on a determination its "ready"
                    you could check for AuthEnabled and AuthReady separately but this makes it a bit easier and you can't forget to check one when there is only one variable */
                    setAuthReady(true);
                }
            }
        })();
    }, [refreshIndex, isLoading, isAuthenticated, configLoaded]);

    function setAuthenticated(idToken, accessToken, username) {
        validateArgs(
            [{idToken}, types.string],
            [{accessToken}, types.string],
            [{username}, types.string, optional]
        );
        setAccessToken(accessToken);
        setIdToken(idToken);
        config.auth0.isAuthenticated = true;
        config.auth0.accessToken = accessToken;
        config.auth0.identityToken = idToken;
        config.auth0.userName = username;
    }
    
    return {
        idToken,
        accessToken,
        authEnabled: !!config.auth0,
        authReady,
        setTokensForcefully(identityToken, accessToken, userName) {
            setAuthenticated(identityToken, accessToken, userName)
        },
        requireAuth(onAuthenticated) {
            validateArgs([{onAuthenticated}, types.function]);
            if (authReady) {
                if (!!config.auth0) {
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
