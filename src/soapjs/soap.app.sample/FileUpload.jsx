import * as React from "react";
import { FileUploader } from "baseui/file-uploader";
import { ListItem, ListItemLabel } from "baseui/list";
import { Button } from "baseui/button";
import Delete from "baseui/icon/delete";
import Download from "baseui/icon/arrow-down"
import Resizer from "react-image-file-resizer";
import { StyledLink } from "baseui/link";
import { Modal, ModalHeader, ModalBody, SIZE, ROLE } from "baseui/modal";
import { useStyletron } from "baseui";

const resizeTo = (blob, { maxHeight, maxWidth }) =>
    new Promise((resolve) => {
        Resizer.imageFileResizer(
            blob,
            maxWidth,
            maxHeight,
            "JPEG",
            100,
            0,
            (uri) => {
                resolve(uri);
            },
            "base64"
        );
    });

function blobToDataURL(blob){
    return new Promise((resolve, reject) => {
        var fr = new FileReader();
        fr.onload = () => {
            resolve(fr.result )
        };
        fr.readAsDataURL(blob);
    });
}

export default (props) => {
    
    const { value, onChange, onBlur, error, acceptedTypes } = props;
    const dimensions = props.dimensions ?? { maxWidth: 1024, maxHeight: 768 };
    const [isUploading, setIsUploading] = React.useState(false);
    const [isLoaded, setIsLoaded] = React.useState(false);
    const [isOpen, setIsOpen] = React.useState(false);
    const [css] = useStyletron();

    if (!isLoaded) {
        onChange(value);
        setIsLoaded(true);
    }
    
    async function handleFile(file) {
        const fileInfo = await new Promise((resolve) => {
            const reader = new FileReader();
            reader.readAsArrayBuffer(file);

            reader.onload = () => {
                const fileInfo = {
                    name: file.name,
                    type: file.type,
                    size: Math.round(file.size / 1000) + " kB",
                    blob: new Blob([reader.result]),
                };

                resolve(fileInfo);
            };
        });
        switch (file.type) {
            case "image/png":
            case "image/jpeg":
            case "image/jfif":
            case "image/gif":
                fileInfo.thumb = await resizeTo(fileInfo.blob, {
                    maxWidth: 100,
                    maxHeight: 100
                });
                fileInfo.fullSize = await resizeTo(fileInfo.blob, {
                    maxWidth: dimensions.maxWidth,
                    maxHeight: dimensions.maxHeight
                });
                const img = new Image();
                img.src = fileInfo.fullSize;
                await new Promise((resolve) => {
                    img.onload = resolve;
                });

                fileInfo.fullSizeHeight = img.height;
                fileInfo.fullSizeWidth = img.width;
                fileInfo.blob = null;
                break;
                
            default:
                fileInfo.blob = await blobToDataURL(fileInfo.blob);
                break;
        }
        return fileInfo;
    }

    function renderUploadedItem(item) {
        if (isUploading) return;
        let thumb, fullSize, fileLink;
        
        if (item) {
            fileLink = item.blob ? <StyledLink
                href={item.blob}
                download={item.name}
            >
                <Download size={20} />
            </StyledLink> : null; 
            thumb = item.thumb ? <img src={item.thumb} alt={item.name} /> : null;
            fullSize = item.fullSize ? (
                <span>
          <StyledLink
              onClick={() => setIsOpen(true)}
              className={css({
                  cursor: "pointer"
              })}
          >
            {item.fullSizeWidth}x{item.fullSizeHeight}
          </StyledLink>
          <Modal
              onClose={() => setIsOpen(false)}
              isOpen={isOpen}
              animate
              autoFocus
              size={SIZE.auto}
              role={ROLE.dialog}
          >
            <ModalHeader>{item.name}</ModalHeader>
            <ModalBody>
              <img src={item.fullSize} alt={item.name} />
            </ModalBody>
          </Modal>
        </span>
            ) : null;
        }
        
        if (item) {
            return (
                <ListItem
                    overrides={{
                        Root: { style: { height: "100px", marginTop: "10px" } }
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
                            <Delete />
                        </Button>
                    )}
                >
                    {thumb}
                    <ListItemLabel>
                        {item.name}&nbsp;
                        {fullSize}
                        {fileLink}
                    </ListItemLabel>
                </ListItem>
            );
        }
    }

    async function sleep(ms) {
        return new Promise((resolve) => setTimeout(resolve, ms));
    }

    return (
        <div>
            <FileUploader
                accept = {props.acceptedTypes}
                onBlur={onBlur}
                multiple={false}
                progressMessage={isUploading ? `Processing... hang tight.` : ""}
                onDrop={async (acceptedFiles, rejectedFiles) => {
                    const start = Date.now();
                    setIsUploading(true);
                    const fileInfo = await handleFile(acceptedFiles[0]);
                    const delta = Date.now() - start; // milliseconds elapsed since start
                    await sleep(Math.max(1000 - delta, 0));
                    setIsUploading(false);
                    onChange(fileInfo);
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
            {renderUploadedItem(value)}
        </div>
    );
};
