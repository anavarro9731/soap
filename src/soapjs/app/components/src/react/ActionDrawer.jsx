import {useAuth} from "../hooks/useAuth";
import React, {useEffect, useState, Fragment} from "react";
import bus from "../soap/bus";
import {Button, KIND, SIZE} from "baseui/button";
import {ANCHOR, Drawer} from "baseui/drawer";
import {Heading, HeadingLevel} from "baseui/heading";
import {Cell, Grid} from "baseui/layout-grid";
import {useStyletron} from "baseui";
import {optional, types, validateArgs} from "../soap/util";


export function ActionDrawer(props) {

    const {title, auth, children, viewHeight=50} = props;
    
    validateArgs(
[{viewHeight}, types.number, optional]
    );
    
    const {requireAuth} = useAuth();
    const [isOpen, setIsOpen] = useState(false);
    const [css, theme] = useStyletron();
    
    useEffect(() => {
        //* register to close yourself, if your children publish closeAllDialogs
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
            bus.closeAllDialogs(); //if you close yourself, close anything behind you (e.g. popover menus). this could be a problem if there were double layered drawers but I don't think we have any, you could always make it a parameter if there were.
            }}
            autoFocus
            size={SIZE.full}
            anchor={ANCHOR.top}
            overrides={viewHeight ? ({
                DrawerContainer: {
                    style: ({ $theme }) => ({                         
                        height:viewHeight+"vh",
                        maxWidth:"fit-content",
                        width:"auto"
                    })
                }}) : undefined
            }
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



  