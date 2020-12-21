import bus from '../soap/bus';
import commandHandler from '../soap/command-handler';

export function useCommand(command) {
    const conversationId = commandHandler.handle(command, () => null, 0);
    bus.closeConversation(conversationId);
}
