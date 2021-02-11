import * as React from "react";
import config from "../soap/config";

//* https://simbathesailor.dev/useismounted-ref-vs-state-approach%20/

//* changing isMounted won't trigger rerender with useRef
export const useIsMounted = () => {
    const isMounted = React.useRef(false);
    React.useEffect(() => {
        console.error("set ismounted to true");
        isMounted.current = true;
        return () => {
            //* should be called when component that uses the hook is unloaded
            console.error("set ismounted to false");
            isMounted.current = false;
        };
    }, []); //* runs only on mount and dismount due to empty array making the effect destructor = component destructor
    return isMounted;
};


export const useIsConfigLoaded = () => {
    
    const [isLoaded, setIsLoaded] = React.useState(config.isLoaded);
    const [callbackAdded, setCallbackAdded] = React.useState(false);
    const isMounted = useIsMounted(); 

    function configLoadedCallback() {
        /* ensure you avoid any stale closure by using a normal function along with ismounted implemented as a ref
           alternatively you could use the pattern of replacing the callback each time useEffect runs, but that runs late and requires adding an id to the callback, its just messier */
        console.error("callback running checking ismounted", isMounted);
        if (isMounted.current === true) {
            console.error("setting config loaded to true, should cause rerender");
            setIsLoaded(true);    
        }
    }

    //* its possible that this block needs to run in useEffect after render but I think it will be safe
    if (!callbackAdded) {
        console.error("adding configloaded callback")
        config.onLoaded(configLoadedCallback);
        setCallbackAdded(true);
    }
    return isLoaded;
};
