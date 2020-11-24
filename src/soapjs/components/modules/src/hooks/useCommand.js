import {bus, commandHandler} from '../soap';

export function useCommand(command) {
    const conversationId = commandHandler.handle(command, () => null, 0);
    bus.closeConversation(conversationId);
}
