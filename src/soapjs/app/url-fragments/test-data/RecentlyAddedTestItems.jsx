import {useCommand, useQuery, useEvent} from "@soap/modules";
import {H5, Paragraph1} from "baseui/typography";
import React, {Fragment, useState} from "react";
import {StyledSpinnerNext} from 'baseui/spinner';
import {Cell, Grid} from "baseui/layout-grid";
import {Modal, ModalBody, ModalButton, ModalFooter, ModalHeader} from "baseui/modal";
import {Button, KIND} from "baseui/button";
import {List} from "baseui/dnd-list";


export function RecentlyAddedTestItems() {

    const [deleteModalIsOpen, setDeleteModelOpen] = useState(false);
    const [rowId, setRowId] = useState();
    const [deletedRowIds, setDeletedRowIds] = useState([]);
    const [rowLabel, setRowLabel] = useState();
    const [command, setCommand] = useState(undefined);
    const [sendCommand, setSendCommand] = useState(false);

    useCommand(command, sendCommand);
    
    function close() {
        setDeleteModelOpen(false);
    }
    
    function deleteRow(id) {
        setCommand({
            $type: 'Soap.Api.Sample.Messages.Commands.C114v1_DeleteTestDataById',
            C114_TestDataId: id
        });
        setSendCommand(true);
        setDeletedRowIds([...deletedRowIds, id]);
        setDeleteModelOpen(false);
    }
    
    let e105 = useQuery({
            query: {
                $type: "Soap.Api.Sample.Messages.Commands.C111v1_GetRecentTestData"
            }
        });

        let listItems;
        if (e105) {
            
            listItems = e105.e105_TestData
                /* filter out deleted ones */.filter(e105 => !deletedRowIds.includes(e105.e105_Id))
                .map(item => (
                <Grid key={item.e105_Id}>
                    <Cell span={8}><Paragraph1>{item.e105_Label} created: {item.e105_CreatedAt}</Paragraph1></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => location.href="#/test-data/view/" + item.e105_Id}>View</Button></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => location.href="#/test-data/edit/" + item.e105_Id}>Edit</Button></Cell>
                    <Cell><Button kind={KIND.minimal} onClick={() => {
                        setRowLabel(item.e105_Label)
                        setRowId(item.e105_Id);
                        setDeleteModelOpen(true);
                    }}>Delete</Button></Cell>
                </Grid>
            ));
        } else {
            listItems = <StyledSpinnerNext/>;
        }     
        
        
        
        
        return (<Fragment>
                <H5>Recently Added Test Items</H5>
                <Modal onClose={close} isOpen={deleteModalIsOpen}>
                    <ModalHeader>Are you sure?</ModalHeader>
                    <ModalBody>
                        Deleting this item "{rowLabel}"set cannot be undone.
                    </ModalBody>
                    <ModalFooter>
                        <ModalButton kind="tertiary" onClick={close}>
                            Cancel
                        </ModalButton>
                        <ModalButton kind="tertiary" onClick={() => deleteRow(rowId)}>
                            OK
                        </ModalButton>
                    </ModalFooter>
                </Modal>
                {listItems}
            </Fragment>
        );
    }
