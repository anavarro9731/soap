import {useStyletron} from "baseui";
import {PLACEMENT, StatefulPopover} from "baseui/popover";
import {Button, KIND, SIZE} from "baseui/button";

import React, {useEffect, useRef} from 'react';
import bus from "../soap/bus";
import {optional, types, uuidv4, validateArgs} from "../soap/util";


export function CreatePrimaryActionsMenu(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );
    if (entityMenus && entityMenus[propertyKey]?.actions) {
        const actions = entityMenus[propertyKey]?.actions;

        return (<PrimaryActionMenu>
            {actions.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
        </PrimaryActionMenu>);
    } else {
        return null;
    }
}

export function CreateSecondaryActionsMenu(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );
    if (entityMenus && entityMenus[propertyKey]?.actions) {
        const actions = entityMenus[propertyKey]?.actions;

        return (<SecondaryActionMenu>
            {actions.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
        </SecondaryActionMenu>);
    } else {
        return null;
    }
}

export function CreateTertiaryActionsMenu(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );
    if (entityMenus && entityMenus[propertyKey]?.actions && entityMenus[propertyKey]?.actions.every(action => action(entity) !== undefined)) {
        const actions = entityMenus[propertyKey]?.actions;

        return (<TertiaryActionMenu>
            {actions.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
        </TertiaryActionMenu>);
    } else {
        return null;
    }
}

export function CreateViewButton(propertyKey, entityMenus, entity) {

    validateArgs(
        [{propertyKey}, types.string],
        [{entityMenus}, types.object, optional],
        [{entity}, types.object, optional]
    );

    if (entityMenus && entityMenus[propertyKey]?.viewAction) {
        const viewAction = entityMenus[propertyKey]?.viewAction;

        if (viewAction instanceof Array) {
            return (<ViewLink>
                {viewAction.map(action => <div key={uuidv4()}>{action(entity)}</div>)}
            </ViewLink>);
        } else {
            return (<Button kind={KIND.secondary} size={SIZE.compact}  overrides={{
                BaseButton: {
                    style: {
                        fontSize:"x-large",
                        paddingLeft:"5px",
                        paddingRight: "5px"
                    }
                }
            }}
                            onClick={() => viewAction(entity)}>{"\uD83D\uDC41"}</Button>);
        }
    } else {
        return null;
    }
}


export const PrimaryActionMenuButton =(props) => <Button kind={KIND.primary} size={SIZE.compact} {...props} />;
export function PrimaryActionMenu(props) {
    const {buttonText = "\u2630"} = props;
    return (
        <ActionMenu children={props.children} button={<Button kind={KIND.primary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "0px"
                }
            }
        }}/>
    );
}

export const SecondaryActionMenuButton =(props) => <Button kind={KIND.primary} size={SIZE.compact} {...props} />;
export function SecondaryActionMenu(props) {
    const {buttonText = "\uFE19"} = props;
    return (
        <ActionMenu children={props.children} button={<Button kind={KIND.primary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "1px"
                }
            }
        }}/>
    );
}

export const TertiaryActionMenuButton =(props) => <Button kind={KIND.primary} size={SIZE.compact} {...props} />;
export function TertiaryActionMenu(props) {
    const {buttonText = "\uFE19"} = props;
    return (
        <ActionMenu children={props.children} button={<Button kind={KIND.primary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "1px"
                }
            }
        }}/>
    );
}

export const ViewLinkButton =(props) => <Button kind={KIND.secondary} size={SIZE.compact} {...props} />;
export function ViewLink(props) {
    const {buttonText = "\uD83D\uDC41"} = props;
    return (
        <ActionMenu children={props.children} button={<Button kind={KIND.secondary} size={SIZE.compact} overrides={{
            BaseButton: {
                style: {
                    fontSize:"x-large",
                    paddingLeft:"5px",
                    paddingRight: "5px"
                }
            }
        }}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "1px"
                }
            }
        }}/>
    );
}

export const ActionMenu = (props) => {

    const {borderWidth, children, button} = props;
    const [css, theme] = useStyletron();
    const sub = useRef();
    
    return (
        children ?
            <StatefulPopover
                dismissOnClickOutside={true}
                placement={PLACEMENT.right}
                overrides={{
                    Body: {
                        style: ({$theme}) => ({
                            boxShadow: 'none',
                        }),
                    }
                }}
                content={({close}) => {
                    
                    //* unsub from previous render's sub
                    if (!!sub.current) {
                        sub.current.unsubscribe();
                    }

                    //* the close method pass into content is different on every render so we need to re-capture it
                    sub.current = bus.onCloseAllDialogs(() => {
                        //* clear previous sub
                        sub.current.unsubscribe();
                        
                        close();
                        }
                    );

                    return <div>{children}</div>
                }}
            >
                {button}
            </StatefulPopover> : null
    );
}
