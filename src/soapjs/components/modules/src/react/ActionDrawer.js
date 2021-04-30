import {useAuth} from "../hooks/useAuth";
import React, {useEffect, useState, Fragment} from "react";
import bus from "../soap/bus";
import {Button, KIND, SIZE} from "baseui/button";
import {ANCHOR, Drawer} from "baseui/drawer";
import {Heading, HeadingLevel} from "baseui/heading";
import {Cell, Grid} from "baseui/layout-grid";
import {useStyletron} from "baseui";


export function ActionDrawer(props) {

    const {title, auth, children} = props;
    const {requireAuth} = useAuth();
    const [isOpen, setIsOpen] = useState(false);
    const [css, theme] = useStyletron();
    
    useEffect(() => {
        //* would be published by children
        bus.onCloseAllDialogs(() => setIsOpen(false));
    }, []); //* run once
    
    return (<Fragment>
        <Button style={{width:"100%"}} size={SIZE.compact} kind={KIND.secondary} onClick={() => {
            if (auth) {
                requireAuth(() => {
                    setIsOpen(true);
                })
            } else {
                setIsOpen(false)
            }
        }} children={title} />
        
        <Drawer 
            isOpen={isOpen} 
            onClose={() => {
            //* when closing via corner X
            setIsOpen(false);
            }}
            autoFocus
            size={SIZE.full}
            anchor={ANCHOR.top}
            >
            <div
                className={css({
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                })}
            >
                <div>
                    <HeadingLevel>
                        <Grid gridGaps={25}>
                            <Cell span={12}><Heading>{title}</Heading></Cell>
                            <Cell span={12}>{children}</Cell>
                        </Grid>
                    </HeadingLevel>
                </div>
            </div>
        </Drawer>
    </Fragment>);
    
    
}