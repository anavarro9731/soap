import {styled, useStyletron, withStyle} from "baseui";
import {PLACEMENT, StatefulPopover} from "baseui/popover";
import {Button, KIND} from "baseui/button";
import {ChevronRight} from "baseui/icon";
import React, {useState} from 'react';
import bus from "../soap/bus";

export function PrimaryActionMenu() {
    return (
        <ActionMenu overrides={{
            Body: {
                ActionMenuContent:
                    {
                        style: {
                            borderWidth: "0px"
                        }
                    }
            }
        }}/>
    );
}

export function SecondaryActionMenu() {
    return (
        <ActionMenu overrides={{
            Body: {
                ActionMenuContent:
                    {
                        style: {
                            borderWidth: "1px"
                        }
                    }
            }
        }}/>
    );
}

const ActionMenu = (props) => {

    const {borderWidth, children, buttonText = "Actions"} = props;
    const [css, theme] = useStyletron();

    const [closeAllDialogsSubscription, setCloseAllDialogsSubscription] = useState(false);
    const [closeFromContent, setCloseFromContent] = useState();

    closeAllDialogsSubscription.unsubscribe();
    setCloseAllDialogsSubscription(bus.onCloseAllDialogs(() => closeFromContent()));

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

                //* the close method pass into content is different on every render so we need to capture it
                setCloseFromContent(close);

                return (
                    <ActionMenuContent>
                        {children}
                    </ActionMenuContent>
                );
            }}
        >
            <Button endEnhancer={ChevronRight} kind={KIND.minimal}>{buttonText}</Button>
        </StatefulPopover> : null
    );
}

const ActionMenuContent = styled('div', ({$theme}) => ({
    backgroundColor: $theme.colors.backgroundPrimary,
    borderStyle: $theme.borders.border600.borderStyle,
    borderColor: $theme.borders.border600.borderColor,
    flexDirection: 'column',
    display: 'flex'
}));
