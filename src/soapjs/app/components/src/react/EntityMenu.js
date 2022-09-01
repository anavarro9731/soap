import {optional, types, validateArgs} from "../soap/util";

export class EntityMenu {
    constructor(viewAction, actions) {

        validateArgs(
            //viewAction > (entity) => action or [(entity) => component]
            [{actions}, [types.function], optional] //(entity) => {}
        );

        this.viewAction = viewAction;
        this.actions = actions;
    }
}
