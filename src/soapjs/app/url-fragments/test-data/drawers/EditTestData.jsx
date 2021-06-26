import {ActionDrawer, AutoForm, bus, translate} from "@soap/modules";
import React from "react";
import wordKeys from "../../../translations/word-keys";


export function EditTestData(props) {
    const {id} = props;
    return (
        <ActionDrawer title={"Edit " + translate(wordKeys.testFormHeader)} auth={true}>
            <AutoForm
                    query={{
                        $type: "C113v1_GetC107FormDataForEdit",
                        c113_TestDataId: id
                    }}
                hiddenFields={["C107_Guid"]}
                submitText="Create"
                afterSubmit={() => bus.closeAllDialogs()}
            />
        </ActionDrawer>
    );
}