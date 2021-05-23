import {ActionModal} from "@soap/modules";
import React from "react";

export function RemoveTestData(props) {

    const {entity} = props;

    return (<ActionModal auth={true} title={`Remove`} command={{
            $type: 'Soap.Api.Sample.Messages.Commands.C114v1_DeleteTestDataById',
            C114_TestDataId: entity.id
        }}>
            {`Do you want to remove ${entity.label}?`}
        </ActionModal>
    );
}