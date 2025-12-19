import { Editor } from '@monaco-editor/react';
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Badge, Button, Card, Col, Container, Form, Modal, Row, Spinner, Stack, Alert } from "react-bootstrap";
import { PencilSquare, Trash, ChevronDown, ChevronRight } from "react-bootstrap-icons";
import { CodeBlock, obsidian } from "react-code-blocks";
import { MockRoute, useProckStore } from '../store/useProckStore';
import { AsyncData } from '../store/AsyncData';
import * as api from '../network/api';



export default function MockRoutes() {
    const mockRoutes: AsyncData<MockRoute[]> = useProckStore((state) => state.mockRoutes);
    const getMockRoutes = useProckStore((state) => state.getMockRoutes);
    const createMockRoute = useProckStore((state) => state.createMockRoute);
    const updateMockRoute = useProckStore((state) => state.updateMockRoute);
    const deleteMockRoute = useProckStore((state) => state.deleteMockRoute);
    const deleteAllMockRoutes = useProckStore((state) => state.deleteAllMockRoutes);

    const [selectedRoute, setSelectedRoute] = useState<MockRoute>();
    const [newRoute, setNewRoute] = useState<MockRoute>();
    const [expandedRoutes, setExpandedRoutes] = useState<Record<string, boolean>>({});


    const [showEditModal, setShowEditModal] = useState(false);


    const handleCloseEditModal = () => {
        setSelectedRoute(undefined);
        setShowEditModal(false);
    }
    const handleShowEditModal = (route: MockRoute) => {
        setSelectedRoute(route);
        setShowEditModal(true);
    }

    const [showCreateModal, setShowCreateModal] = useState(false);

    const handleCloseCreateModal = () => {
        setNewRoute(undefined);
        setShowCreateModal(false);
    }
    const handleShowCreateModal = () => {
        setNewRoute({});
        setShowCreateModal(true);
    }

    const [showDeleteModal, setShowDeleteModal] = useState(false);

    const handleCloseDeleteModal = () => {
        setSelectedRoute(undefined);
        setShowDeleteModal(false);
    }
    const handleShowDeleteModal = (route: MockRoute) => {
        setSelectedRoute(route);
        setShowDeleteModal(true);
    }

    const [showDeleteAllModal, setShowDeleteAllModal] = useState(false);

    const handleDeleteAllRoutes = async () => {
        await deleteAllMockRoutes();
        setShowDeleteAllModal(false);
    }

    const handleSubmitNewRoute = async () => {
        // Validate
        if (!newRoute?.mock) {
            console.error("no new mock to submit!");
            return;
        }
        createMockRoute(newRoute);
        handleCloseCreateModal();
    }

    const handleSubmitUpdateRoute = async () => {
        // Validate
        if (!selectedRoute?.mock) {
            console.error("no new mock to submit!");
            return;
        }
        updateMockRoute(selectedRoute);
        handleCloseEditModal();
    }

    const handleDeleteRoute = async () => {
        // Validate
        if (!selectedRoute?.routeId) {
            console.error("no mock to delete!");
            return;
        }
        deleteMockRoute(selectedRoute.routeId);
        handleCloseDeleteModal();
    }


    function isJsonString(str?: string) {
        if (!str) return false;
        try {
            JSON.parse(str);
            // eslint-disable-next-line @typescript-eslint/no-unused-vars
        } catch (error) {
            return false;
        }
        return true;
    }

    function renderMethodBadge(method: string) {
        method = method.toUpperCase();
        let variant = "";
        switch (method) {
            case "GET":
                variant = "info";
                break;
            case "POST":
                variant = "success";
                break;
            case "PUT":
                variant = "primary";
                break;
            case "PATCH":
                variant = "warning";
                break;
            case "DELETE":
                variant = "danger";
                break;
            default:
                variant = "secondary";
                break;
        }
        return (
            <Badge bg={variant} className='me-3' pill>{method.toUpperCase()}</Badge>
        )
    }

    useEffect(() => {
        if (mockRoutes.value == undefined && !mockRoutes.isLoading && !mockRoutes.isError) {
            getMockRoutes();
        }
    }, [getMockRoutes, mockRoutes.isError, mockRoutes.isLoading, mockRoutes.value]);



    return <Container fluid className='mt-3'>
        <Row className="mb-3">
            <Col>
                <h4>Mock Routes</h4>
                <p className="text-muted">Manage functionality and responses for your mock routes.</p>
            </Col>
            <Col xs="auto">
                <Stack direction="horizontal" gap={2}>
                    {mockRoutes.value && mockRoutes.value.length > 0 && (
                        <Button variant="outline-danger" onClick={() => setShowDeleteAllModal(true)} data-testid="delete-all-routes-btn">
                            Delete All
                        </Button>
                    )}
                    <Button variant="outline-primary" onClick={handleShowCreateModal} data-testid="create-route-btn">
                        Add Route
                    </Button>
                </Stack>
            </Col>
        </Row>
        {mockRoutes.value ?
            (mockRoutes.value.length > 0 ? (
                <Stack gap={3}>
                    {mockRoutes.value.map((route) => {
                        const isExpanded = !!expandedRoutes[route.routeId!];
                        return (
                            <Card key={route.routeId} data-testid={`mock-route-card-${route.routeId}`}>
                                <Card.Header
                                    onClick={() => route.routeId && setExpandedRoutes(prev => ({ ...prev, [route.routeId!]: !prev[route.routeId!] }))}
                                    style={{ cursor: 'pointer' }}
                                >
                                    <Row className="align-items-center">
                                        <Col xs="auto">
                                            {isExpanded ? <ChevronDown /> : <ChevronRight />}
                                        </Col>
                                        <Col>
                                            <Card.Title className="mb-0">
                                                {route.method && renderMethodBadge(route.method)}
                                                {route.path}
                                            </Card.Title>
                                        </Col>
                                        <Col xs={2} onClick={(e) => e.stopPropagation()}>
                                            <Form.Check
                                                type="switch"
                                                id={`${route.routeId}-switch`}
                                                data-testid={`mock-route-switch-${route.routeId}`}
                                                className='float-end'
                                                checked={route.enabled}
                                                label={route.enabled ? "Enabled" : "Disabled"}
                                                onChange={async () => {
                                                    if (route.routeId === undefined || route.enabled === undefined)
                                                        return;
                                                    if (!route.enabled) {
                                                        await api.enableRouteAsync(route.routeId);
                                                    }
                                                    else if (route.enabled) {
                                                        await api.disableRouteAsync(route.routeId);
                                                    }
                                                }} />
                                        </Col>
                                    </Row>
                                </Card.Header>
                                {isExpanded && (
                                    <>
                                        <Card.Body>
                                            <Card.Subtitle className='mt-2'>
                                                Mock {route.httpStatusCode} Response:
                                            </Card.Subtitle>
                                            <CodeBlock
                                                language="json"
                                                text={JSON.stringify(route.mock, null, 2)}
                                                theme={obsidian}
                                                showLineNumbers={false}
                                            />

                                        </Card.Body>
                                        <Card.Footer>

                                            <Stack direction='horizontal' gap={2} className="float-end">
                                                <Button size='sm' variant='secondary' onClick={() => handleShowDeleteModal(route)} data-testid={`delete-route-btn-${route.routeId}`}>
                                                    <Trash />
                                                </Button>
                                                <Button size='sm' onClick={() => handleShowEditModal(route)} data-testid={`edit-route-btn-${route.routeId}`}>
                                                    <PencilSquare />
                                                </Button>
                                            </Stack>
                                        </Card.Footer>
                                    </>
                                )}
                            </Card>
                        )
                    })}
                </Stack>
            ) : (
                <div className="text-center p-4">
                    <Alert variant='warning' className="text-muted">No mock routes found. Create a mock route or generate mock routes from an OpenAPI spec.</Alert>
                </div>
            ))
            :
            <>
                {/* <div className="d-flex justify-content-around"><p>{errorMessage}</p></div> */}
                <div className="d-flex justify-content-around"><Spinner className='m-4 text-center' variant='warning' /></div>
            </>
        }
        <Modal show={showDeleteModal} onHide={handleCloseDeleteModal}>
            <Modal.Header closeButton>
                <Modal.Title>Delete Route</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {selectedRoute &&
                    <>
                        <p>Are you sure you want to delete the following route?</p>
                        <p><b>{selectedRoute.method}</b> {selectedRoute.path}</p>
                    </>
                }
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleCloseDeleteModal}>
                    Cancel
                </Button>
                <Button variant="primary" onClick={handleDeleteRoute} disabled={!selectedRoute?.routeId} data-testid="confirm-delete-route-btn">
                    Delete
                </Button>
            </Modal.Footer>
        </Modal>
        <Modal show={showDeleteAllModal} onHide={() => setShowDeleteAllModal(false)}>
            <Modal.Header closeButton>
                <Modal.Title>Delete All Routes</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <p>Are you sure you want to delete <strong>all {mockRoutes.value?.length || 0} mock routes</strong>?</p>
                <p className="text-danger">This action cannot be undone.</p>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={() => setShowDeleteAllModal(false)}>
                    Cancel
                </Button>
                <Button variant="danger" onClick={handleDeleteAllRoutes} data-testid="confirm-delete-all-routes-btn">
                    Delete All
                </Button>
            </Modal.Footer>
        </Modal>
        <Modal show={showCreateModal} onHide={handleCloseCreateModal} fullscreen={true}>
            <Modal.Header closeButton>
                <Modal.Title>Add Route</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {newRoute &&
                    <Container>
                        <Form.Group className="mb-3">
                            <Form.Label>HTTP Method</Form.Label>
                            <Form.Select
                                value={newRoute.method}
                                data-testid="route-method-select"
                                onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setNewRoute({ ...newRoute, method: e.currentTarget.value })}>
                                <option value={""}>Select...</option>
                                <option value="Get">Get</option>
                                <option value="Post">Post</option>
                                <option value="Put">Put</option>
                                <option value="Delete">Delete</option>
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Path</Form.Label>
                            <Form.Control
                                type="text"
                                placeholder="/some/path"
                                value={newRoute.path}
                                data-testid="route-path-input"
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setNewRoute({ ...newRoute, path: e.currentTarget.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Http Status Code</Form.Label>
                            <Form.Control
                                type="number"
                                placeholder="200"
                                value={newRoute.httpStatusCode}
                                data-testid="route-status-input"
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setNewRoute({ ...newRoute, httpStatusCode: e.currentTarget.valueAsNumber })} />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Mock Response</Form.Label>
                            <Editor
                                theme="vs-dark"
                                height={"500px"}
                                defaultLanguage="json"
                                defaultValue={JSON.stringify(newRoute.mock, null, 2)}
                                onChange={(value) => setNewRoute({ ...newRoute, mock: isJsonString(value) ? JSON.parse(value as string) : newRoute.mock })}
                            />
                        </Form.Group>
                    </Container>
                }
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleCloseCreateModal}>
                    Cancel
                </Button>
                <Button variant="primary" onClick={handleSubmitNewRoute} disabled={
                    !newRoute?.method || !newRoute.path || !newRoute.mock || !newRoute.httpStatusCode
                } data-testid="save-route-btn">
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
        <Modal show={showEditModal} onHide={handleCloseEditModal} fullscreen={true}>
            <Modal.Header closeButton>
                <Modal.Title>Edit Mock Route</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {selectedRoute &&
                    <Container>
                        <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                            <Form.Label>HTTP Method</Form.Label>
                            <Form.Select value={selectedRoute.method}
                                onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setSelectedRoute({ ...selectedRoute, method: e.currentTarget.value })}>
                                <option value={""}>Select...</option>
                                <option value="GET">GET</option>
                                <option value="POST">POST</option>
                                <option value="PUT">PUT</option>
                                <option value="DELETE">DELETE</option>
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                            <Form.Label>Path</Form.Label>
                            <Form.Control type="text"
                                placeholder="/some/path"
                                value={selectedRoute.path}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSelectedRoute({ ...selectedRoute, path: e.currentTarget.value })} />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Http Status Code</Form.Label>
                            <Form.Control
                                type="number"
                                placeholder="200"
                                value={selectedRoute.httpStatusCode}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSelectedRoute({ ...selectedRoute, httpStatusCode: e.currentTarget.valueAsNumber })} />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Mock Response</Form.Label>
                            <Editor
                                theme="vs-dark"
                                height={"500px"}
                                defaultLanguage="json"
                                defaultValue={JSON.stringify(selectedRoute.mock, null, 2)}
                                onChange={(value) =>
                                    setSelectedRoute({ ...selectedRoute, mock: isJsonString(value) ? JSON.parse(value as string) : selectedRoute.mock })}
                            />
                        </Form.Group>
                    </Container>
                }

            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleCloseEditModal}>
                    Cancel
                </Button>
                <Button variant="primary" onClick={handleSubmitUpdateRoute}>
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
    </Container>;
}