import React, { useState, useEffect, useRef } from 'react';
import {
    Button,
    Card,
    Col,
    Container,
    Row,
    Table,
    Modal,
    Form,
    Alert,
    Badge,
    Spinner
} from 'react-bootstrap';
import { useOpenApiStore, OpenApiDocument, CreateOpenApiDocument } from '../../store/useOpenApiStore';
import JsonModal from './JsonModal';

const OpenApiDocuments: React.FC = () => {
    const {
        documents,
        getDocuments,
        createDocument,
        updateDocument,
        deleteDocument,
        fetchOpenApiJson
    } = useOpenApiStore();

    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [showJsonModal, setShowJsonModal] = useState(false);
    const [selectedDocument, setSelectedDocument] = useState<OpenApiDocument | null>(null);
    const [createForm, setCreateForm] = useState<CreateOpenApiDocument>({
        title: '',
        version: '',
        description: '',
        openApiJson: ''
    });
    const [uploadError, setUploadError] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        getDocuments();
    }, [getDocuments]);

    useEffect(() => {
        if (selectedDocument) {
            fetchOpenApiJson(selectedDocument.documentId);
        }
    }, [fetchOpenApiJson, selectedDocument]);

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setUploadError(null);

        if (!createForm.openApiJson.trim()) {
            setUploadError('OpenAPI JSON is required');
            return;
        }

        try {
            // Try to parse JSON to validate it
            JSON.parse(createForm.openApiJson);
            await createDocument(createForm);
            setShowCreateModal(false);
            setCreateForm({ title: '', version: '', description: '', openApiJson: '' });
        } catch {
            setUploadError('Invalid JSON format');
        }
    };

    const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (event) => {
                const content = event.target?.result as string;
                setCreateForm(prev => ({ ...prev, openApiJson: content }));

                // Try to extract title and version from the JSON
                try {
                    const parsed = JSON.parse(content);
                    if (parsed.info) {
                        setCreateForm(prev => ({
                            ...prev,
                            title: prev.title || parsed.info.title || '',
                            version: prev.version || parsed.info.version || '',
                            description: prev.description || parsed.info.description || ''
                        }));
                    }
                } catch {
                    // Ignore parsing errors for now
                }
            };
            reader.readAsText(file);
        }
    };

    const handleDelete = async (documentId: string) => {
        if (window.confirm('Are you sure you want to delete this OpenAPI document?')) {
            await deleteDocument(documentId);
        }
    };

    const handleToggleActive = async (document: OpenApiDocument) => {
        await updateDocument(document.documentId, { isActive: !document.isActive });
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString() + ' ' + new Date(dateString).toLocaleTimeString();
    };

    const ViewDocumentModal = () => (
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
                <Button variant="secondary" onClick={() => setShowDetailModal(false)}>
                    Close
                </Button>
                <Button variant="secondary" onClick={() => {
                    setShowJsonModal(true);
                    setShowDetailModal(false);
                }
                }>
                    View JSON
                </Button>
            </Modal.Footer>
        </Modal>
    );

    return (
        <Container className="mt-3" fluid>
            <Row className="mb-3">
                <Col>
                    <h4>OpenAPI Documents</h4>
                    <p className="text-muted">Manage your OpenAPI specifications</p>
                </Col>
                <Col xs="auto">
                    <Button variant="primary" onClick={() => setShowCreateModal(true)}>
                        Upload New Document
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
                                                variant="outline-primary"
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
                            <p className="text-muted">No OpenAPI documents found.</p>
                            <Button variant="primary" onClick={() => setShowCreateModal(true)}>
                                Upload Your First Document
                            </Button>
                        </div>
                    )}
                </Card.Body>
            </Card>

            {/* Create Document Modal */}
            <Modal show={showCreateModal} onHide={() => setShowCreateModal(false)} size="lg">
                <Modal.Header closeButton>
                    <Modal.Title>Upload OpenAPI Document</Modal.Title>
                </Modal.Header>
                <Form onSubmit={handleCreateSubmit}>
                    <Modal.Body>
                        {uploadError && <Alert variant="danger">{uploadError}</Alert>}

                        <Row className="mb-3">
                            <Col md={6}>
                                <Form.Group className="mb-3">
                                    <Form.Label>Title</Form.Label>
                                    <Form.Control
                                        type="text"
                                        value={createForm.title}
                                        onChange={(e) => setCreateForm(prev => ({ ...prev, title: e.target.value }))}
                                        placeholder="API Title (optional)"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={6}>
                                <Form.Group className="mb-3">
                                    <Form.Label>Version</Form.Label>
                                    <Form.Control
                                        type="text"
                                        value={createForm.version}
                                        onChange={(e) => setCreateForm(prev => ({ ...prev, version: e.target.value }))}
                                        placeholder="1.0.0 (optional)"
                                    />
                                </Form.Group>
                            </Col>
                        </Row>

                        <Form.Group className="mb-3">
                            <Form.Label>Description</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={2}
                                value={createForm.description}
                                onChange={(e) => setCreateForm(prev => ({ ...prev, description: e.target.value }))}
                                placeholder="API Description (optional)"
                            />
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label>Upload OpenAPI File</Form.Label>
                            <Form.Control
                                ref={fileInputRef}
                                type="file"
                                accept=".json,.yaml,.yml"
                                onChange={handleFileUpload}
                            />
                            <Form.Text className="text-muted">
                                Select a JSON or YAML file containing your OpenAPI specification
                            </Form.Text>
                        </Form.Group>

                        <Form.Group className="mb-3">
                            <Form.Label>OpenAPI JSON/YAML *</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={10}
                                value={createForm.openApiJson}
                                onChange={(e) => setCreateForm(prev => ({ ...prev, openApiJson: e.target.value }))}
                                placeholder="Paste your OpenAPI specification here..."
                                required
                            />
                        </Form.Group>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button variant="secondary" onClick={() => setShowCreateModal(false)}>
                            Cancel
                        </Button>
                        <Button variant="primary" type="submit">
                            Upload Document
                        </Button>
                    </Modal.Footer>
                </Form>
            </Modal>

            <ViewDocumentModal />
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
