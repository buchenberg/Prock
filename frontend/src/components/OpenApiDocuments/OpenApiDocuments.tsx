import React, { useState, useEffect } from 'react';
import {
    Button,
    Card,
    Col,
    Container,
    Row,
    Table,
    Alert,
    Badge,
    Spinner,
    Modal
} from 'react-bootstrap';
import { useOpenApiStore, OpenApiDocument } from '../../store/useOpenApiStore';
import ScalarModal from './ScalarModal';
import { formatDate } from '../../helpers/functions';
import ViewDocumentModal from './ViewDocumentModal';
import CreateDocumentModal from './CreateDocumentModal';

const OpenApiDocuments: React.FC = () => {
    const {
        documents,
        getDocuments,
        deleteDocument,
        fetchOpenApiJson
    } = useOpenApiStore();

    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [showScalarModal, setShowScalarModal] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<OpenApiDocument | null>(null);

    // Delete Modal State
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [documentToDelete, setDocumentToDelete] = useState<OpenApiDocument | null>(null);

    useEffect(() => {
        getDocuments();
    }, [getDocuments]);

    useEffect(() => {
        if (selectedDocument) {
            fetchOpenApiJson(selectedDocument.documentId);
        }
    }, [fetchOpenApiJson, selectedDocument]);


    const handleShowDeleteModal = (document: OpenApiDocument) => {
        setDocumentToDelete(document);
        setShowDeleteModal(true);
    };

    const handleCloseDeleteModal = () => {
        setDocumentToDelete(null);
        setShowDeleteModal(false);
    };

    const handleConfirmDelete = async () => {
        if (documentToDelete) {
            deleteDocument(documentToDelete.documentId);
            handleCloseDeleteModal();
        }
    };

    return (
        <Container className="mt-3" fluid>
            <Row className="mb-3">
                <Col>
                    <h4>OpenAPI Documents</h4>
                    <p className="text-muted">Upload and manage all of your OpenAPI specifications here.</p>
                    <p className="text-muted">Once you have uploaded a specification document, you can view, edit, delete, or generate mocks from the document.</p>

                </Col>
                <Col xs="auto">
                    <Button variant="outline-primary" onClick={() => setShowCreateModal(true)} data-testid="upload-doc-btn">
                        Upload Document
                    </Button>
                </Col>
            </Row>

            {documents.isError && (
                <Alert variant="danger">
                    Error uploading document: {documents.errorMessage}
                </Alert>
            )}

            <Card>
                <Card.Body>
                    {documents.isLoading ? (
                        <div className="text-center p-4">
                            <Spinner animation="border" role="status">
                                <span className="visually-hidden">Loading...</span>
                            </Spinner>
                        </div>
                    ) : documents.value && documents.value.length > 0 ? (
                        <Table striped hover responsive>
                            <thead>
                                <tr>
                                    <th>Title</th>
                                    <th>Version</th>
                                    <th>Description</th>
                                    <th>Created</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {documents.value.map((doc: OpenApiDocument) => (
                                    <tr key={doc.documentId} data-testid={`openapi-doc-row-${doc.documentId}`}>
                                        <td>
                                            <strong>{doc.title || 'Untitled'}</strong>
                                        </td>
                                        <td>
                                            <Badge bg="info">{doc.version}</Badge>
                                        </td>
                                        <td className="text-truncate" style={{ maxWidth: '200px' }}>
                                            {doc.description || '-'}
                                        </td>
                                        <td>{formatDate(doc.createdAt)}</td>
                                        <td>
                                            <Button
                                                variant="outline-primary"
                                                size="sm"
                                                className="me-2"
                                                onClick={() => {
                                                    setSelectedDocument(doc);
                                                    setShowDetailModal(true);
                                                }}
                                                data-testid={`view-doc-btn-${doc.documentId}`}
                                            >
                                                View
                                            </Button>
                                            <Button
                                                variant="outline-danger"
                                                size="sm"
                                                onClick={() => handleShowDeleteModal(doc)}
                                                data-testid={`delete-doc-btn-${doc.documentId}`}
                                            >
                                                Delete
                                            </Button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                    ) : (
                        <div className="text-center p-4">
                            <Alert variant='warning' className="text-muted">No documents found. Upload one to get started.</Alert>
                        </div>
                    )}
                </Card.Body>
            </Card>

            {/* Create Document Modal */}
            <CreateDocumentModal
                show={showCreateModal}
                onHide={() => setShowCreateModal(false)}
            />
            {/* View Document Modal */}
            <ViewDocumentModal
                showDetailModal={showDetailModal}
                setShowDetailModal={setShowDetailModal}
                selectedDocument={selectedDocument}
                setShowScalarModal={setShowScalarModal}
            />
            {/* Scalar Modal */}
            <ScalarModal
                title={documents.value?.find(d => d.documentId === selectedDocument?.documentId)?.title || ''}
                showScalarModal={showScalarModal}
                documentId={selectedDocument?.documentId || ''}
                onHide={() => setShowScalarModal(false)}
            />

            {/* Delete Confirmation Modal */}
            <Modal show={showDeleteModal} onHide={handleCloseDeleteModal}>
                <Modal.Header closeButton>
                    <Modal.Title>Delete OpenAPI Document</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    Are you sure you want to delete the document <strong>{documentToDelete?.title || 'Untitled'}</strong>?
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseDeleteModal}>
                        Cancel
                    </Button>
                    <Button variant="danger" onClick={handleConfirmDelete} data-testid="confirm-delete-doc-btn">
                        Delete
                    </Button>
                </Modal.Footer>
            </Modal>
        </Container>
    );
};

export default OpenApiDocuments;
