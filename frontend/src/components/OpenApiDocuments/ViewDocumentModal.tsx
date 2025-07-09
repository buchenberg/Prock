import { Modal, Row, Col, Badge, Button } from "react-bootstrap";
import { OpenApiDocument } from "../../store/useOpenApiStore";
import { formatDate } from "../../helpers/functions";
import { useProckStore } from "../../store/useProckStore";



const ViewDocumentModal = ({ showDetailModal, setShowDetailModal, selectedDocument, setShowJsonModal }: {
    showDetailModal: boolean;
    setShowDetailModal: (show: boolean) => void;
    selectedDocument: OpenApiDocument | null;
    setShowJsonModal: (show: boolean) => void;
}) => {
    const { generateMockRoutesFromOpenApi } = useProckStore();

    return (
        <Modal show={showDetailModal} onHide={() => setShowDetailModal(false)} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>OpenAPI Document Details</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {selectedDocument && (
                    <div>
                        <h5>{selectedDocument.title}</h5>
                        <p className="text-muted">{selectedDocument.description}</p>

                        <Row className="mb-3">
                            <Col sm={6}>
                                <strong>Version:</strong> {selectedDocument.version}
                            </Col>
                            <Col sm={6}>
                                <strong>OpenAPI Version:</strong> {selectedDocument.openApiVersion}
                            </Col>
                        </Row>

                        <Row className="mb-3">
                            <Col sm={6}>
                                <strong>Created:</strong> {formatDate(selectedDocument.createdAt)}
                            </Col>
                            <Col sm={6}>
                                <strong>Updated:</strong> {formatDate(selectedDocument.updatedAt)}
                            </Col>
                        </Row>

                        {selectedDocument.host && (
                            <Row className="mb-3">
                                <Col sm={6}>
                                    <strong>Host:</strong> {selectedDocument.host}
                                </Col>
                                <Col sm={6}>
                                    <strong>Base Path:</strong> {selectedDocument.basePath || '/'}
                                </Col>
                            </Row>
                        )}

                        {selectedDocument.schemes && selectedDocument.schemes.length > 0 && (
                            <div className="mb-3">
                                <strong>Schemes:</strong>{' '}
                                {selectedDocument.schemes.map(scheme => (
                                    <Badge key={scheme} bg="secondary" className="me-1">{scheme}</Badge>
                                ))}
                            </div>
                        )}
                    </div>
                )}
            </Modal.Body>
            <Modal.Footer>
                <Button variant="outline-secondary" onClick={() => setShowDetailModal(false)}>
                    Close
                </Button>
                <Button variant="outline-primary" onClick={() => {
                    setShowJsonModal(true);
                    setShowDetailModal(false);
                }
                }>
                    View JSON
                </Button>
                <Button
                    variant="outline-primary"
                    onClick={() => generateMockRoutesFromOpenApi(selectedDocument?.documentId || '')}
                >
                    Generate Mock Routes
                </Button>

            </Modal.Footer>
        </Modal>
    )
}
export default ViewDocumentModal;