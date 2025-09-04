
import { Modal, Button, Container } from "react-bootstrap";
import { JsonView, allExpanded, darkStyles } from "react-json-view-lite";
import 'react-json-view-lite/dist/index.css';
import { useEffect } from "react";
import { useOpenApiStore } from "../../store/useOpenApiStore";

const JsonModal = ({ onHide, showJsonModal, title, documentId }: {
    onHide: () => void;
    showJsonModal: boolean;
    title?: string;
    documentId: string;
}) => {
    const { documentDetail, fetchOpenApiJson } = useOpenApiStore();

    useEffect(() => {
        if (documentId) {
            fetchOpenApiJson(parseInt(documentId));
        }
    }, [documentId, fetchOpenApiJson]);

    return (
        <Modal show={showJsonModal} fullscreen onHide={onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>{title || 'OpenAPI JSON'}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Container id="json-container">
                    <JsonView
                        data={documentDetail.value || ""}
                        shouldExpandNode={allExpanded}
                        style={darkStyles}
                    />
                </Container>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}
export default JsonModal;