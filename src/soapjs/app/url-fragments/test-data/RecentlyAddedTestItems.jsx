import {useCommand, useQuery} from "@soap/modules";
import {H5, Paragraph1} from "baseui/typography";
import React, {Fragment, useState, useReducer} from "react";
import {StyledSpinnerNext} from 'baseui/spinner';
import {Cell, Grid} from "baseui/layout-grid";
import {Modal, ModalBody, ModalButton, ModalFooter, ModalHeader} from "baseui/modal";
import {Button, KIND} from "baseui/button";

export function RecentlyAddedTestItems() {

    const [state, dispatch] = useReducer(function reducer(state, action) {
        switch (action.type) {
            case 'cancelDelete':
                return {...state,
                    deleteModalOpen: false
                };
            case 'confirm':
                //* itemId, itemLabel
                return {
                    step: 'confirm',
                    itemId : action.itemId, 
                    deletedRowIds : state.deletedRowIds,
                    deleteModalOpen: true,
                    rowLabel: action.itemLabel,
                    command: undefined,
                    sendCommand: false
                };
            case 'delete':
                //* command
                return {
                step: 'delete',
                itemId : state.itemId,
                deletedRowIds : [...state.deletedRowIds, state.itemId],
                deleteModalOpen: false,
                rowLabel: state.rowLabel,
                command: action.command,
                sendCommand: true
            };
            default:
                throw new Error();
        }
    }, {
        step: null,
        itemId: null,
        deletedRowIds: [],
        deleteModalOpen: false,
        rowLabel: null,
        sendCommand: false,
        command: undefined
    });
    
    useCommand(state.command, state.sendCommand);
    
    function close() {
        dispatch({type: 'cancelDelete'});
    }
    
    function deleteRow() {
        dispatch({
            type: 'delete',
            command: {
                $type: 'Soap.Api.Sample.Messages.Commands.C114v1_DeleteTestDataById',
                C114_TestDataId: state.itemId
            }
        });
    }
    
    let e105 = useQuery({
            query: {
                $type: "Soap.Api.Sample.Messages.Commands.C111v1_GetRecentTestData"
            }
        });

        let listItems;
        if (e105) {
            
            listItems = e105.e105_TestData
                /* filter out deleted ones */.filter(e105 => !state.deletedRowIds.includes(e105.e105_Id))
                .map(item => (
                <Grid key={item.e105_Id}>
                    <Cell span={8}><Paragraph1>{item.e105_Label} created: {item.e105_CreatedAt}</Paragraph1></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => location.href="#/test-data/view/" + item.e105_Id}>View</Button></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => location.href="#/test-data/edit/" + item.e105_Id}>Edit</Button></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => {
                        dispatch({
                            type: 'confirm',
                            itemLabel:item.e105_Label,
                            itemId:item.e105_Id 
                        });
                    }}>Delete</Button></Cell>
                </Grid>
            ));
        } else {
            listItems = <StyledSpinnerNext/>;
        }     
        
        return (<Fragment>
                <H5>Recently Added Test Items</H5>
                <Modal onClose={close} isOpen={state.deleteModalOpen}>
                    <ModalHeader>Are you sure?</ModalHeader>
                    <ModalBody>
                        Deleting this item "{state.itemLabel}"set cannot be undone.
                    </ModalBody>
                    <ModalFooter>
                        <ModalButton kind="tertiary" onClick={close}>
                            Cancel
                        </ModalButton>
                        <ModalButton kind="tertiary" onClick={deleteRow}>
                            OK
                        </ModalButton>
                    </ModalFooter>
                </Modal>
                {listItems}
            </Fragment>
        );
    }
