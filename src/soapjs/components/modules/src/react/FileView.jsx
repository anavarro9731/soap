import * as React from "react";
import {useEffect, useState} from "react";
import {ListItem, ListItemLabel} from "baseui/list";
import {Button} from "baseui/button";
import Download from "baseui/icon/arrow-down"
import {StyledLink} from "baseui/link";
import {Modal, ModalBody, ModalHeader, ROLE, SIZE} from "baseui/modal";
import {useStyletron} from "baseui";
import {uuidv4} from "../soap/util";
import Compressor from 'compressorjs'
import config from "../soap/config";

function resizeTo(blob, {maxHeight, maxWidth}) {
    return new Promise((resolve, reject) => {
        new Compressor(blob, {
            checkOrientation: false,
            maxHeight: maxHeight,
            maxWidth: maxWidth,
            success: resolve,
            error: reject
        });
    });
}


async function objectUrlToBlob(objectUrl) {
    let blob = await fetch(objectUrl).then(r => r.blob());
    return blob;
}

export default (props) => {

    const {value} = props;
    
    const [isLoading, setIsLoading] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const [enrichedBlob, setEnrichedBlob] = useState(null);
    
    const dimensions = props.dimensions ?? {maxWidth: 1024, maxHeight: 768};
    const [css] = useStyletron();
    

    //* run to get the blob state after first render is complete
    useEffect(() => {
        (async function GetBlobFromBackend() {
            if (value && value.objectUrl === undefined) {
                setIsLoading(true);
                const endpoint = `${config.vars.functionAppRoot}/GetBlob`;
                let response = await fetch(`${endpoint}?id=${encodeURI(value.id)}`);
                const blob = await response.blob();
                const blobInfo = {
                    id: value.id,
                    name: value.name,
                    blob: blob
                };
                
                const enrichedBlob = await enrichBlobInfo(blobInfo);
                setEnrichedBlob(enrichedBlob);
                setIsLoading(false);
            }
        })();
    }, []) //* run only once
   
    return (
        <div>
            {renderUploadedItem()}
        </div>
    );

    async function enrichBlobInfo(blobInfo) {

        switch (blobInfo.blob.type) {
            case "image/png":
            case "image/jpeg":
            case "image/jpg":
            case "image/jfif":

                blobInfo.thumb = URL.createObjectURL(await resizeTo(blobInfo.blob, {
                    maxWidth: 100,
                    maxHeight: 100
                }));

                const resizedBlob = await resizeTo(blobInfo.blob, {
                    maxWidth: dimensions.maxWidth,
                    maxHeight: dimensions.maxHeight
                });

                const objectUrl = URL.createObjectURL(resizedBlob);

                const img = new Image();
                img.src = objectUrl;

                await new Promise((resolve) => {
                    img.onload = resolve;
                });

                blobInfo.height = img.height;
                blobInfo.width = img.width;
                blobInfo.objectUrl = objectUrl;
                blobInfo.isImage = true;
                break;

            default:
                blobInfo.isImage = false;
                blobInfo.objectUrl = URL.createObjectURL(blobInfo.blob);
                break;
        }
        blobInfo.sizeInKb = Math.round(blobInfo.blob.size / 1000) + " kb";
        delete blobInfo.blob;
        return blobInfo;
    }


    function renderUploadedItem() {

        if (isLoading) return;  //* show nothing while preparing value

        if (enrichedBlob) { //* could be null on empty new form

            let thumb, fullSize, file;
            if (enrichedBlob.isImage) {
                thumb = <img src={enrichedBlob.thumb} alt={enrichedBlob.name}/>;
                fullSize = (
                    <span>
          <StyledLink
              onClick={() => setIsOpen(true)}
              className={css({
                  cursor: "pointer"
              })}
          >
            {enrichedBlob.width}x{enrichedBlob.height}
          </StyledLink>
          <Modal
              onClose={() => setIsOpen(false)}
              isOpen={isOpen}
              animate
              autoFocus
              size={SIZE.auto}
              role={ROLE.dialog}
          >
            <ModalHeader>{enrichedBlob.name}</ModalHeader>
            <ModalBody>
              <img src={enrichedBlob.objectUrl} alt={enrichedBlob.name}/>
            </ModalBody>
          </Modal>
        </span>
                );
            } else {
                file = enrichedBlob.objectUrl ? <StyledLink
                    href={enrichedBlob.objectUrl}
                    download={enrichedBlob.name}
                >
                    <Download size={20}/>
                </StyledLink> : null;
            }

            return (
                <div>
                    {thumb}
                    {enrichedBlob.name}&nbsp;({enrichedBlob.sizeInKb})&nbsp;
                    {fullSize}
                    {file}
                </div>
                
            );
        }
    }

};
