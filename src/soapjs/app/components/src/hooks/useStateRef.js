import {useState, useRef, useEffect} from 'react';

/* 
gives a state like variable that will give you the current value in a useEffect closure
 */

const useRefState = (initialValue) => {
    const [state, setState] = useState(initialValue);
    const stateRef = useRef(state);
    useEffect(
        () => {
            stateRef.current = state;
        },
        [state],
    );
    return [stateRef, setState];
};
