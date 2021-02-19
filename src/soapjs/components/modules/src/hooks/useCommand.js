import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";
import {useEffect} from "react";
import {useAuth} from "./useLogin";
import {toTypeName} from "../soap/messages";

export function useCommand(command, sendCommand = true) {
    
    const configLoaded = useIsConfigLoaded("useCommand.js");
    const { authReady } = useAuth();
    
    useEffect(() => {
        
        if (configLoaded && authReady && sendCommand) {
            command.$type = toTypeName(command.$type); //convert from class short name to assembly qualified short name
            if (!command.headers) {
                command.headers = [];
            }
            const conversationId = commandHandler.handle(command, () => null, 0);
            bus.closeConversation(conversationId);
        }
        
    }, [configLoaded, authReady, sendCommand]);
    
}
