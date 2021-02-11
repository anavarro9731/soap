import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import config from "../soap/config";

export const useLogin = () => {

    const [idToken, setIdToken] = useState(null);
    const [accessToken, setAccessToken] = useState(null);

    const {
        isLoading,
        isAuthenticated,
        getIdTokenClaims,
        getAccessTokenSilently,
        user,
        getAccessTokenWithPopup
    } = useAuth0();

    const [refreshIndex, setRefreshIndex] = useState(0);
    
    useEffect(() => {
        (async () => {
            
            if (!isLoading && isAuthenticated) {
                
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

        })();
    }, [refreshIndex, isLoading, isAuthenticated]);

    return {
        idToken,
        accessToken,
        refresh() {
            setRefreshIndex(refreshIndex + 1)
        }
    };
}
