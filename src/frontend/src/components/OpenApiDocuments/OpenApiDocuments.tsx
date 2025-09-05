import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Button,
    Card,
    Col,
    Container,
    Row,
    Table,
    Alert,
    Badge,
    Spinner
} from 'react-bootstrap';
import { useOpenApiStore, OpenApiDocument } from '../../store/useOpenApiStore';
import JsonModal from './JsonModal';
import { formatDate } from '../../helpers/functions';
import ViewDocumentModal from './ViewDocumentModal';
import CreateDocumentModal from './CreateDocumentModal';
import EditDocumentModal from './EditDocumentModal';

const OpenApiDocuments: React.FC = () => {
    const navigate = useNavigate();
    const {
        documents,
        getDocuments,
        updateDocument,
        deleteDocument,
        fetchOpenApiJson
    } = useOpenApiStore();

    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [showJsonModal, setShowJsonModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<OpenApiDocument | null>(null);

    useEffect(() => {
        getDocuments();
    }, [getDocuments]);

    useEffect(() => {
        if (selectedDocument) {
            fetchOpenApiJson(parseInt(selectedDocument.documentId));
        }
    }, [fetchOpenApiJson, selectedDocument]);


    const handleDelete = async (documentId: string) => {
        if (window.confirm('Are you sure you want to delete this OpenAPI document?')) {
            deleteDocument(parseInt(documentId));
        }
    };

    const handleToggleActive = async (document: OpenApiDocument) => {
        updateDocument(parseInt(document.documentId), { isActive: !document.isActive });
    };

    const handleNavigateToMocks = () => {
        // Navigate to the mocks tab using hash routing
        navigate('#mocks');
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
                    <Button variant="outline-primary" onClick={() => setShowCreateModal(true)}>
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
                                    <th>Status</th>
                                    <th>Created</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {documents.value.map((doc: OpenApiDocument) => (
                                    <tr key={doc.documentId}>
                                        <td>
                                            <strong>{doc.title || 'Untitled'}</strong>
                                        </td>
                                        <td>
                                            <Badge bg="info">{doc.version}</Badge>
                                        </td>
                                        <td className="text-truncate" style={{ maxWidth: '200px' }}>
                                            {doc.description || '-'}
                                        </td>
                                        <td>
                                            <Badge bg={doc.isActive ? 'success' : 'secondary'}>
                                                {doc.isActive ? 'Active' : 'Inactive'}
                                            </Badge>
                                        </td>
                                        <td>{formatDate(doc.createdAt)}</td>
                                        <td>
                                            <Button
                                                variant="outline-info"
                                                size="sm"
                                                className="me-2"
                                                onClick={() => {
                                                    setSelectedDocument(doc);
                                                    setShowDetailModal(true);
                                                }}
                                            >
                                                View
                                            </Button>
                                            <Button
                                                variant={'outline-primary'}
                                                size="sm"
                                                className="me-2"
                                                 onClick={() => {
                                                    setSelectedDocument(doc);
                                                    setShowEditModal(true);
                                                }}
                                            >
                                                {'Edit'}
                                            </Button>
                                            <Button
                                                variant={doc.isActive ? 'outline-warning' : 'outline-success'}
                                                size="sm"
                                                className="me-2"
                                                onClick={() => handleToggleActive(doc)}
                                            >
                                                {doc.isActive ? 'Deactivate' : 'Activate'}
                                            </Button>
                                            <Button
                                                variant="outline-danger"
                                                size="sm"
                                                onClick={() => handleDelete(doc.documentId)}
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
                setShowJsonModal={setShowJsonModal}
                onNavigateToMocks={handleNavigateToMocks}
            />
            {/* Edit Document Modal */}
            <EditDocumentModal
                showEditModal={showEditModal}
                setShowEditModal={setShowEditModal}
                selectedDocument={selectedDocument}
            />
            {/* JSON Modal */}
            <JsonModal
                title={documents.value?.find(d => d.documentId === selectedDocument?.documentId)?.title || ''}
                showJsonModal={showJsonModal}
                documentId={selectedDocument?.documentId || ''}
                onHide={() => setShowJsonModal(false)}
            />
        </Container>
    );
};

export default OpenApiDocuments;
