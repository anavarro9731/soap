import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";
import {useEffect} from "react";
import {useAuth} from "./useAuth";
import {toTypeName} from "../soap/messages";

export function useCommand(command, sendCommand = true, closeConversation = true) {
    
    const configLoaded = useIsConfigLoaded("useCommand.js");
    const { authReady } = useAuth();
    
    let conversationId;
    
    useEffect(() => {
        
        if (configLoaded && authReady && sendCommand) {
            const typeName = toTypeName(command.$type); //* convert from class short name to assembly qualified short name 
            if (command.$type !== typeName) { //* should be equal on proxy classes created from registered messages, important because you cannot modify proxied properties
                command.$type = typeName;
            } 
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
