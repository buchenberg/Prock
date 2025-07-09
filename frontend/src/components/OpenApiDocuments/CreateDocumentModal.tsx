import { useRef, useState } from "react";
import { Modal, Form, Alert, Row, Col, Button } from "react-bootstrap";
import { CreateOpenApiDocument, useOpenApiStore } from "../../store/useOpenApiStore";

const CreateDocumentModal = ({ show, onHide }: {
    show: boolean;
    onHide: () => void;
}) => {
    const {
        createDocument,
    } = useOpenApiStore();
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [uploadError, setUploadError] = useState<string | null>(null);
    const [createForm, setCreateForm] = useState<CreateOpenApiDocument>({
        title: '',
        version: '',
        description: '',
        openApiJson: ''
    });
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
            createDocument(createForm);
            onHide();
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
    return (
        <Modal show={show} onHide={onHide} size="lg">
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
                    <Button variant="secondary" onClick={onHide}>
                        Cancel
                    </Button>
                    <Button variant="primary" type="submit">
                        Upload Document
                    </Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}

export default CreateDocumentModal;