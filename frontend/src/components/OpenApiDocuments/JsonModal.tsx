
import { Modal, Button } from "react-bootstrap";
import SpecViewer from "./SpecViewer";

const JsonModal = ({ onHide, showJsonModal, title, documentId }: {
    onHide: () => void;
    showJsonModal: boolean;
    title?: string;
    documentId: string;
}) => {
    return (
            <Modal show={showJsonModal} fullscreen onHide={onHide} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>{title || 'OpenAPI JSON'}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <SpecViewer documentId={documentId} />
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