import {ActionDrawer, AutoForm, bus, translate} from "@soap/modules";
import React from "react";
import wordKeys from "../../../translations/word-keys";


export function CreateTestData(props) {
    return (
        <ActionDrawer title={"Create " + translate(wordKeys.testFormHeader)} auth={true}>
            <AutoForm
                query={{
                    $type: "C109v1_GetC107FormDataForCreate"
                }}
                submitText="Create"
                afterSubmit={() => bus.closeAllDialogs()}
            />
        </ActionDrawer>
    );
}

