import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";
import {useEffect} from "react";
import {useAuth} from "./useLogin";
import {toTypeName} from "../soap/messages";

export function useCommand(command, sendCommand = true, closeConversation = true) {
    
    const configLoaded = useIsConfigLoaded("useCommand.js");
    const { authReady } = useAuth();
    
    let conversationId;
    
    useEffect(() => {
        
        if (configLoaded && authReady && sendCommand) {
            command.$type = toTypeName(command.$type); //convert from class short name to assembly qualified short name
            if (!command.headers) {
                command.headers = [];
            }
            conversationId = commandHandler.handle(command, () => null, 0);
            if (closeConversation) {
                bus.closeConversation(conversationId);
            }
        }
        
    }, [configLoaded, authReady, sendCommand]);
    
    return conversationId;
}
