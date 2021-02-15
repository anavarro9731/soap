import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";
import {useEffect} from "react";
import {useAuth} from "./useLogin";

export function useCommand(command, sendCommand = true) {
    
    const configLoaded = useIsConfigLoaded("useCommand.js");
    const { authReady } = useAuth();
    
    useEffect(() => {
        
        if (configLoaded && authReady && sendCommand) {
            const conversationId = commandHandler.handle(command, () => null, 0);
            bus.closeConversation(conversationId);
        }
        
    }, [configLoaded, authReady, sendCommand]);
    
}
