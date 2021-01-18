import React from "react";
import {useAuth0} from "@auth0/auth0-react";
import {Button} from "baseui/button";

const Login = () => {

    const {logout, loginWithRedirect, user, isAuthenticated, isLoading} = useAuth0();

    if (isLoading) {
        return;
    }

    if (isAuthenticated) {
        return (<React.Fragment>
            <span>{user.name}</span>
            <Button onClick={() => logout({returnTo: window.location.origin})}>Logout</Button>
        </React.Fragment>);
    } else {
        return <Button onClick={() => loginWithRedirect()}>Log In</Button>;
    }
};

export default Login;
