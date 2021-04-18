import React, {Fragment, useEffect, useState} from "react";
import {Button, KIND, SIZE} from "baseui/button";
import {Modal, ModalBody, ModalButton, ModalFooter, ModalHeader} from "baseui/modal";
import {useAuth} from "../hooks/useAuth";
import {useCommand} from "../hooks/useCommand";
import bus from "../soap/bus";

export function ActionModal(props) {

    const {title, auth, children, command} = props;

    const {requireAuth} = useAuth();
    const [modalIsOpen, setModalIsOpen] = useState(false);
    const [useCommandArgs, setUseCommandArgs] = useState({command, sendCommand: false});

    useCommand(useCommandArgs.command, useCommandArgs.sendCommand);

    const [closeDialogs, setCloseDialogs] = useState(false);

    useEffect(() => {
        if(closeDialogs) {
            bus.closeAllDialogs();
        }
    });
    
    return (<Fragment>
        <Button kind={KIND.tertiary} onClick={() => {
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
                <ModalButton kind={KIND.secondary} onClick={
                    () => {
                        setModalIsOpen(false);
                        setCloseDialogs(true);
                    }
                }>
                    No
                </ModalButton>
                <ModalButton kind={KIND.secondary} onClick={
                    () => {
                        setUseCommandArgs(current => ({
                            ...current,
                            sendCommand: true
                        }));
                        setCloseDialogs(true);
                    }
                }>
                    Yes
                </ModalButton>
            </ModalFooter>
        </Modal>
    </Fragment>);
}