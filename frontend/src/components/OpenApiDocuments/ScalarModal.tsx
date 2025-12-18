import { Modal, Button, Container } from "react-bootstrap";
import { ApiReferenceReact } from "@scalar/api-reference-react";
import { useOpenApiStore } from "../../store/useOpenApiStore";
import { useEffect } from "react";
import '@scalar/api-reference-react/style.css';

import { useNavigate } from "react-router-dom";

const ScalarModal = ({ onHide, showScalarModal, title, documentId }: {
    onHide: () => void;
    showScalarModal: boolean;
    title?: string;
    documentId: string;
}) => {
    const { documentDetail, fetchOpenApiJson } = useOpenApiStore();
    const navigate = useNavigate();

    const handleClose = () => {
        onHide();
        navigate('#openapi');
    };

    useEffect(() => {
        if (showScalarModal && documentId) {
            fetchOpenApiJson(documentId);
        }
    }, [documentId, fetchOpenApiJson, showScalarModal]);

    return (
        <Modal show={showScalarModal} fullscreen onHide={handleClose} size="lg">
            <Modal.Header closeButton>
                <Modal.Title>{title || 'API Documentation'} {typeof documentDetail?.value === 'object' && documentDetail?.value?.info?.version ? `- ${documentDetail.value.info.version}` : ''}</Modal.Title>
            </Modal.Header>
            <Modal.Body style={{ overflow: "hidden", padding: 0 }}>
                {documentDetail?.value ? (
                    <div style={{ height: '100%', overflowY: 'auto' }}>
                        <ApiReferenceReact
                            configuration={{
                                content: typeof documentDetail.value === 'string'
                                    ? documentDetail.value
                                    : JSON.stringify(documentDetail.value),
                                theme: 'purple',
                                layout: 'modern',
                                hideDarkModeToggle: true
                            }}
                        />
                    </div>
                ) : (
                    <Container className="p-4 text-center">Loading specification...</Container>
                )}
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleClose}>
                    Close
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ScalarModal;


