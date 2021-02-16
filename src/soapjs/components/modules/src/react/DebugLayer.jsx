import {Layer} from 'baseui/layer';
import {useStyletron} from 'baseui';
import React from "react";

export default function DebugLayer(props) {
    const [css] = useStyletron();
    const {offset, children, forwardedRef} = props;
    
    return (
        <Layer>
        <div
            className={css({
                position: 'fixed',
                top: offset || '1%',
                left: offset || '1%',
                width: '200px',
                paddingTop: '20px',
                paddingBottom: '20px',
                paddingLeft: '20px',
                paddingRight: '20px',
                backgroundColor: "rgba(255, 190, 190, 0.86)",
                textAlign: 'center',
            })}
            ref={forwardedRef}
        >
            {children}
        </div>
        </Layer>
    );
}
