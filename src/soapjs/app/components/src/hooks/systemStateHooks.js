import * as React from "react";
import config from "../soap/config";

//* https://simbathesailor.dev/useismounted-ref-vs-state-approach%20/

//* changing isMounted won't trigger rerender with useRef
export const useIsMounted = (callerId) => {
    
    const isMounted = React.useRef(false);
    
    React.useEffect(() => {
        if (config.debugSystemState) console.warn(`tell "${callerId ?? "unknown"}" that the component is mounted`);
        isMounted.current = true;
        return () => {
            //* should be called when component that uses the hook is unloaded
            if (config.debugSystemState) console.warn(`tell "${callerId ?? "unknown"}" that the component has been unmounted`);
            isMounted.current = false;
        };
    }, []); //* runs only on mount and dismount due to empty array making the effect destructor = component destructor
    
    return isMounted;
};


export const useIsConfigLoaded = (callerId) => {
    
    const [isLoaded, setIsLoaded] = React.useState(config.isLoaded);
    const [callbackAdded, setCallbackAdded] = React.useState(false);
    const isMounted = useIsMounted(callerId); 

    /* its possible that this block needs to run in useEffect after render but I think it will be safe from a timing perspective, though
    I am not sure if setting callbackadded will cause midstream rerender in parent component or avoid that because it is only used in this
    hook, and hooks don't render anything I am going to guess that its safe */ 
     
    if (!callbackAdded && !isLoaded) {
        
        if (config.debugSystemState) console.warn(`Adding config onLoaded callback  ["${callerId ?? "unknown"}"]`);
        
        config.onLoaded(configLoadedCallback);
        setCallbackAdded(true);
    }
    return isLoaded;

    function configLoadedCallback() {
        /* ensure you avoid any stale closure by using a normal function along with ismounted implemented as a ref
           alternatively you could use the pattern of replacing the callback each time useEffect runs, but that runs late and requires adding an id to the callback, its just messier */
        if (config.debugSystemState) console.warn(`onLoaded callback started, checking isMounted ["${callerId ?? "unknown"}"]`, isMounted);
        if (isMounted.current === true) {
            if (config.debugSystemState) console.warn(`setting config loaded to true, should cause rerender ["${callerId ?? "unknown"}"]`);
            setIsLoaded(true);
        }
    }
    
};
