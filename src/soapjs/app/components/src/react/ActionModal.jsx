import React, {Fragment, useEffect, useState} from "react";
import {Button, KIND, SIZE} from "baseui/button";
import {Modal, ModalBody, ModalButton, ModalFooter, ModalHeader} from "baseui/modal";
import {useAuth} from "../hooks/useAuth";
import {useCommand} from "../hooks/useCommand";
import bus from "../soap/bus";
import {optional, types, validateArgs} from "../soap/util";
import {CenterSpinner} from "./CenterSpinner";

export function ActionModal(props) {

    const {title, afterSubmitHref, cancelText, submitText, auth, children, command} = props;
    
    validateArgs(
        [{afterSubmitHref}, types.string, optional],
        [{cancelText}, types.string, optional],
        [{submitText}, types.string, optional]
    )
    
    const {requireAuth} = useAuth("ActionModal");
    const [modalIsOpen, setModalIsOpen] = useState(false);
    const [useCommandArgs, setUseCommandArgs] = useState({command, sendCommand: false});
    const [submitted, setSubmitted] = useState(false);
    
    useCommand(useCommandArgs.command, useCommandArgs.sendCommand);

    const [closeDialogs, setCloseDialogs] = useState(false);

    useEffect(() => {
        if(closeDialogs) {
            bus.closeAllDialogs();
        }
        if (submitted && !!afterSubmitHref) {
            location.href=afterSubmitHref;
        }
    });
    
    return submitted ? <CenterSpinner/> : (<Fragment>
        <Button style={{width:"100%"}} size={SIZE.compact} kind={KIND.secondary} onClick={() => {
            if (auth) {
                requireAuth(() => {
                    setModalIsOpen(true);
                })
            } else {
                setModalIsOpen(true);
            }
        }
        }>{title}</Button>

        <Modal onClose={() => {
            //close via corner x
            setModalIsOpen(false);
            setCloseDialogs(true);
        }} isOpen={modalIsOpen}>
            <ModalHeader>{title}</ModalHeader>
            <ModalBody>
                {children}
            </ModalBody>
            <ModalFooter>
                <ModalButton kind={KIND.secondary} size={SIZE.compact} onClick={
                    () => {
                        setModalIsOpen(false); 
                        setCloseDialogs(true); 
                    }
                }>
                    {cancelText ?? "No"}
                </ModalButton>
                <ModalButton kind={KIND.secondary} size={SIZE.compact} onClick={
                    () => {
                        setUseCommandArgs(current => ({
                            ...current,
                            sendCommand: true
                        }));
                        
                            setCloseDialogs(true);  
                            setSubmitted(true);
                        
                        
                    }
                }>
                    {submitText ?? "Yes"}
                </ModalButton>
            </ModalFooter>
        </Modal>
    </Fragment>);
}