import * as React from "react";
import {useEffect, useState} from "react";
import {FileUploader} from "baseui/file-uploader";
import {ListItem, ListItemLabel} from "baseui/list";
import {Button} from "baseui/button";
import Delete from "baseui/icon/delete";
import Download from "baseui/icon/arrow-down"
import {StyledLink} from "baseui/link";
import {Modal, ModalBody, ModalHeader, ROLE, SIZE} from "baseui/modal";
import {useStyletron} from "baseui";
import {uuidv4} from "../soap/util";
import Compressor from 'compressorjs'
import {Label3} from "baseui/typography";
import config from "../soap/config";
import {useAuth} from "../hooks/useAuth.js"


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

async function uploadBlobToBackend(blobId, blob, idToken) {

    const endpoint = `${config.vars.functionAppRoot}/AddBlob`;
    
    
    //* shouldn't need use effect as page should be loaded
    await fetch(`${endpoint}?id=${blobId}&it=${idToken}`, {
        method: "post",
        //we don’t set Content-Type header manually, because a Blob object has a built-in type for Blob objects that type becomes the value of Content-Type.
        body: blob
    });

    
}

async function objectUrlToBlob(objectUrl) {
    let blob = await fetch(objectUrl).then(r => r.blob());
    return blob;
}

export function FileUpload (props) {

    const {onChange, onBlur, error, acceptedTypes, value, disabled} = props;
    const [isUploading, setIsLoading] = useState(false);
    
    const dimensions = props.dimensions ?? {maxWidth: 1024, maxHeight: 768};
    const [isOpen, setIsOpen] = useState(false);
    const [css] = useStyletron();

    const {
        idToken,
        authReady
    } = useAuth("FileUpload");
    
    //* run to get the blob state after first render is complete
    useEffect(() => {
        (async function GetBlobFromBackend() {
            if (value !== null && value.objectUrl === undefined && authReady) {
                setIsLoading(true);
                const endpoint = `${config.vars.functionAppRoot}/GetBlob`;
                const url = `${endpoint}?id=${encodeURI(value.id)}&it=${idToken}`;
                if (config.debugSystemState) {
                    console.warn("Fetching Blob at: " + url);
                }
                let response = await fetch(url);
                const blob = await response.blob();
                const blobInfo = {
                    id: value.id,
                    name: value.name,
                    blob: blob
                };
                
                const enrichedBlob = await enrichBlobInfo(blobInfo);
                
                onChange(enrichedBlob);  //* forces react hook form to save value in controller
                setIsLoading(false);
            }
        })();
    }, [authReady])
   
    return (
        <div>
            <FileUploader
                accept={acceptedTypes}
                disabled={disabled}
                onBlur={onBlur}
                multiple={false}
                progressMessage={isUploading ? `Processing... hang tight.` : ""}
                onDrop={async (acceptedFiles, rejectedFiles) => {
                    setIsLoading(true);
                    const blob = await getBlobFromDisk(acceptedFiles[0]);
                    const enrichedBlob = await enrichBlobInfo(blob);
                    enrichedBlob.id = uuidv4();
                    const blobToUpload = await objectUrlToBlob(enrichedBlob.objectUrl);
                    await uploadBlobToBackend(enrichedBlob.id, blobToUpload, idToken);
                    onChange(enrichedBlob);
                    setIsLoading(false);
                }}
                overrides={{
                    FileDragAndDrop: {
                        style: (props) => ({
                            borderLeftColor: error
                                ? props.$theme.colors.borderNegative
                                : props.$theme.colors.primaryA,
                            borderRightColor: error
                                ? props.$theme.colors.borderNegative
                                : props.$theme.colors.primaryA,
                            borderTopColor: error
                                ? props.$theme.colors.borderNegative
                                : props.$theme.colors.primaryA,
                            borderBottomColor: error
                                ? props.$theme.colors.borderNegative
                                : props.$theme.colors.primaryA,
                            backgroundColor: error
                                ? props.$theme.colors.backgroundLightNegative
                                : props.$theme.colors.backgroundStateDisabled
                        })
                    }
                }}
            />
            {renderUploadedItem()}
        </div>
    );


    async function getBlobFromDisk(file) {

        const blobInfo = await new Promise((resolve) => {
            const reader = new FileReader();
            reader.readAsArrayBuffer(file);

            reader.onload = () => {
                const blobInfo = {
                    name: file.name,
                    id: uuidv4(),
                    blob: new Blob([reader.result], {type: file.type})
                };
                resolve(blobInfo);
            };
        });

        return blobInfo;
    }

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

        if (isUploading) return;  //* show nothing while preparing value

        if (value && value.objectUrl !== undefined) { //* could be null on empty new form

            if (disabled) {
                return (<Label3>Uploaded {value.name}</Label3>);
            }

            let thumb, fullSize, file;
            if (value.isImage) {
                thumb = <img src={value.thumb} alt={value.name}/>;
                fullSize = (
                    <span>
          <StyledLink
              onClick={() => setIsOpen(true)}
              className={css({
                  cursor: "pointer"
              })}
          >
            {value.width}x{value.height}
          </StyledLink>
          <Modal
              onClose={() => setIsOpen(false)}
              isOpen={isOpen}
              animate
              autoFocus
              size={SIZE.auto}
              role={ROLE.dialog}
          >
            <ModalHeader>{value.name}</ModalHeader>
            <ModalBody>
              <img src={value.objectUrl} alt={value.name}/>
            </ModalBody>
          </Modal>
        </span>
                );
            } else {
                file = value.objectUrl ? <StyledLink
                    href={value.objectUrl}
                    download={value.name}
                >
                    <Download size={20}/>
                </StyledLink> : null;
            }

            return (
                <ListItem
                    overrides={{
                        Root: {style: {height: "100px", marginTop: "10px"}}
                    }}
                    endEnhancer={() => (
                        <Button
                            shape="round"
                            size="compact"
                            kind="secondary"
                            onClick={() => {
                                onChange(null);
                            }}
                        >
                            <Delete/>
                        </Button>
                    )}
                >
                    {thumb}
                    <ListItemLabel>
                        {value.name}&nbsp;({value.sizeInKb})&nbsp;
                        {fullSize}
                        {file}
                    </ListItemLabel>
                </ListItem>
            );
        }
    }

}
