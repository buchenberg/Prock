import { Modal, Row, Col, Badge, Button, Alert, FormGroup, Form } from "react-bootstrap";
import { OpenApiDocument, useOpenApiStore } from "../../store/useOpenApiStore";
import { formatDate } from "../../helpers/functions";
import { useEffect, useState } from "react";



const EditDocumentModal = ({ showEditModal, setShowEditModal, selectedDocument }: {
    showEditModal: boolean;
    setShowEditModal: (show: boolean) => void;
    selectedDocument: OpenApiDocument | null;

}) => {
    const {
        updateDocument
    } = useOpenApiStore();

    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);
    const [formData, setFormData] = useState<
        {
            title: string;
            description: string;
            version: string;
            openApiVersion: string;
            isActive: boolean;
        }>();

    useEffect(() => {
        if (selectedDocument) {
            setFormData({
                title: selectedDocument.title || '',
                description: selectedDocument.description || '',
                version: selectedDocument.version || '',
                openApiVersion: selectedDocument.openApiVersion || '',
                isActive: selectedDocument.isActive || false
            });
        }
    }, [selectedDocument]);

    if (!selectedDocument || !formData) {
        return null; // Don't render the modal if no form data exists
    }

    const handleSaveChanges = async () => {
        setIsSaving(true);
        updateDocument(parseInt(selectedDocument.documentId), formData);
        setShowEditModal(false);
        setErrorMessage(null); // Clear error when closing modal
        setIsSaving(false);
    };

    const formIsDirty = formData && (
        formData.title !== (selectedDocument?.title || '') ||
        formData.description !== (selectedDocument?.description || '') ||
        formData.version !== (selectedDocument?.version || '') ||
        formData.openApiVersion !== (selectedDocument?.openApiVersion || '') ||
        formData.isActive !== (selectedDocument?.isActive || false)
    );




    return (
        <Modal show={showEditModal} onHide={() => {
            setShowEditModal(false);
            setErrorMessage(null); // Clear error when closing modal
        }} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>Edit OpenAPI Document Details</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {errorMessage && (
                    <Alert variant="danger" onClose={() => setErrorMessage(null)} dismissible>
                        {errorMessage}
                    </Alert>
                )}
                {selectedDocument && formData && (
                    <div>
                        <FormGroup className="mb-3">
                            <Form.Label>Title</Form.Label>
                            <Form.Control
                                type="text"
                                value={formData.title}
                                onChange={(e) => {
                                    setFormData({ ...formData, title: e.target.value });
                                }}
                            />
                        </FormGroup>
                        <FormGroup className="mb-3">
                            <Form.Label>Description</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={3}
                                value={formData.description}
                                onChange={(e) => {
                                    setFormData({ ...formData, description: e.target.value });

                                }}
                            />
                        </FormGroup>
                        <FormGroup className="mb-3">
                            <Form.Label>Version</Form.Label>
                            <Form.Control
                                type="text"
                                value={formData.version}
                                onChange={(e) => {
                                    setFormData({ ...formData, version: e.target.value });
                                }}
                            />
                        </FormGroup>
                        <FormGroup className="mb-3">
                            <Form.Label>OpenAPI Version</Form.Label>
                            <Form.Control
                                type="text"
                                value={formData.openApiVersion}
                                onChange={(e) => {
                                    setFormData({ ...formData, openApiVersion: e.target.value });
                                }}
                            />
                        </FormGroup>

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
                <Button variant="outline-primary" onClick={handleSaveChanges} disabled={!formIsDirty || isSaving}>
                    Save Changes
                </Button>
                <Button variant="outline-secondary" onClick={() => {
                    setShowEditModal(false);
                    setErrorMessage(null); // Clear error when closing modal
                }}>
                    Close
                </Button>


            </Modal.Footer>
        </Modal>
    )
}
export default EditDocumentModal;