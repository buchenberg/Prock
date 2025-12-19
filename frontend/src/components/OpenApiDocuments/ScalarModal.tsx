import { Modal, Button, Container } from "react-bootstrap";
import { ApiReferenceReact } from "@scalar/api-reference-react";
import { useOpenApiStore } from "../../store/useOpenApiStore";
import { useProckStore } from "../../store/useProckStore";
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
    const { prockConfig, getProckConfigs } = useProckStore();
    const navigate = useNavigate();

    const handleClose = () => {
        onHide();
        navigate('#openapi');
    };

    useEffect(() => {
        getProckConfigs();
    }, [getProckConfigs]);

    useEffect(() => {
        if (showScalarModal && documentId) {
            fetchOpenApiJson(documentId);
        }
    }, [documentId, fetchOpenApiJson, showScalarModal]);

    const serverUrl = prockConfig.value ? (() => {
        let host = prockConfig.value.host;
        if (host.includes('*')) {
            host = host.replace('*', 'localhost');
        }
        if (!host.startsWith('http')) {
            host = `http://${host}`;
        }
        return `${host}:${prockConfig.value.port}`;
    })() : '';

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
                                servers: serverUrl ? [{ url: serverUrl, description: 'Prock Proxy Server' }] : undefined,
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


