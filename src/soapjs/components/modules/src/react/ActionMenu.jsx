import {useStyletron} from "baseui";
import {PLACEMENT, StatefulPopover} from "baseui/popover";
import {Button, KIND, SIZE} from "baseui/button";
import {ChevronRight} from "baseui/icon";
import React from 'react';
import bus from "../soap/bus";

export const PrimaryActionMenuButton =(props) => <Button kind={KIND.primary} size={SIZE.compact} {...props} />;
export function PrimaryActionMenu(props) {
    const {buttonText = "Act"} = props;
    return (
        <ActionMenu children={props.children} button={<Button endEnhancer={ChevronRight} kind={KIND.primary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
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
    const {buttonText = "Act"} = props;
    return (
        <ActionMenu children={props.children} button={<Button endEnhancer={ChevronRight} kind={KIND.primary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "1px"
                }
            }
        }}/>
    );
}

export const ViewMenuButton =(props) => <Button kind={KIND.secondary} size={SIZE.compact} {...props} />;
export function ViewMenu(props) {
    const {buttonText = "View"} = props;
    return (
        <ActionMenu children={props.children} button={<Button endEnhancer={ChevronRight} kind={KIND.secondary} size={SIZE.compact}>{buttonText}</Button>} overrides={{
            Body: {
                style: {
                    borderWidth: "1px"
                }
            }
        }}/>
    );
}

const ActionMenu = (props) => {

    const {borderWidth, children, button} = props;
    const [css, theme] = useStyletron();

    //const [closeAllDialogsSubscription, setCloseAllDialogsSubscription] = useState();
    //const [closeFromContent, setCloseFromContent] = useState();
    //
    // useEffect(() => {
    //     bus.onCloseAllDialogs(() => closeFromContent());
    //     // const sub = bus.onCloseAllDialogs(() => closeFromContent());
    //     // //setCloseAllDialogsSubscription(sub);
    //     // if (closeAllDialogsSubscription) {
    //     //     closeAllDialogsSubscription.unsubscribe();    
    //     // }    
    // })
    //

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
                    bus.onCloseAllDialogs(close);

                    return <div>{children}</div>
                }}
            >
                {button}
            </StatefulPopover> : null
    );
}
