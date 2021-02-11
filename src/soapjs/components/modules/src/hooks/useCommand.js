import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';
import {useIsConfigLoaded} from "./systemStateHooks";

export function useCommand(command) {
    const configLoaded = useIsConfigLoaded();
    
    if (configLoaded) {
        const conversationId = commandHandler.handle(command, () => null, 0);
        bus.closeConversation(conversationId);
    }
}
