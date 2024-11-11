import { Editor } from '@monaco-editor/react';
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Badge, Button, Card, Col, Container, Form, Modal, Row, Spinner, Stack } from "react-bootstrap";
import { PencilSquare, PlusCircle, Trash } from "react-bootstrap-icons";
import { CodeBlock, obsidian } from "react-code-blocks";
import * as api from '../network/api';
import axios from 'axios';

export interface IMockRoute {
    enabled?: boolean;
    routeId?: string;
    method?: string;
    path?: string;
    mock?: object;
}

export default function MockRoutes() {

    const [routes, setRoutes] = useState<IMockRoute[]>();
    const [selectedRoute, setSelectedRoute] = useState<IMockRoute>();
    const [newRoute, setNewRoute] = useState<IMockRoute>();
    const [showEditModal, setShowEditModal] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");

    const handleCloseEditModal = () => {
        setSelectedRoute(undefined);
        setShowEditModal(false);
    }
    const handleShowEditModal = (route: IMockRoute) => {
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
    const handleShowDeleteModal = (route: IMockRoute) => {
        setSelectedRoute(route);
        setShowDeleteModal(true);
    }

    const handleSubmitNewRoute = async () => {
        // Validate mock
        if (!newRoute?.mock) {
            console.error("no new mock to submit!");
            return;
        }

        try {
            const response = await api.createNewRouteAsync(newRoute);
            const json = await response.data as IMockRoute;
            setRoutes([
                ...(routes as []),
                json
            ]);
            handleCloseCreateModal();
        }
        catch (error: unknown) {
            handleCloseCreateModal();
            if (axios.isAxiosError(error)) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }

    const handleSubmitUpdateRoute = async () => {
        // Validate mock
        if (!selectedRoute?.mock) {
            console.error("no new mock to submit!");
            return;
        }
        try {
            const response = await api.updateRouteAsync(selectedRoute);
            const updatedRoute = response.data as IMockRoute;
            setRoutes(routes?.map((route) => (route.routeId === updatedRoute.routeId) ? updatedRoute : route));
            handleCloseEditModal();
        }
        catch (error: unknown) {
            handleCloseEditModal();
            if (axios.isAxiosError(error)) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }

    const handleDeleteRoute = async () => {
        // Validate mock
        if (!selectedRoute?.routeId) {
            console.error("no mock to delete!");
            return;
        }
        try {
            await api.deleteRouteAsync(selectedRoute.routeId);
            setRoutes(routes?.filter((route) => (route.routeId !== selectedRoute.routeId)));
            handleCloseDeleteModal();
        }
        catch (error: unknown) {
            handleCloseDeleteModal();
            if (axios.isAxiosError(error)) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }

    const getRoutes = async () => {
        try {
            const response = await api.fetchRoutesAsync();
            setRoutes(response.data);
        }
        catch (error: unknown) {
            handleCloseEditModal();
            if (axios.isAxiosError(error)) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }

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
            <Badge bg={variant} className='me-3' pill>{method}</Badge>
        )
    }

    useEffect(() => {
        if (!routes) {
            getRoutes();
        }
    }, [routes]);



    return <>
        {routes ?
            <>
                <div className='mb-3'>
                    <Stack direction='horizontal' gap={2} className='mt-3'>
                        <h4>Mock Routes</h4>
                        <PlusCircle className='icon-btn' onClick={handleShowCreateModal} />
                    </Stack>
                </div>
                <Stack gap={3}>
                    {routes.map((route) => {
                        return (
                            <Card key={route.routeId}>
                                <Card.Header>
                                    <Row>
                                        <Col>
                                            <Card.Title>
                                                {route.method && renderMethodBadge(route.method)}
                                                {route.path}
                                            </Card.Title>
                                        </Col>
                                        <Col xs={2}>
                                            <Form.Check
                                                type="switch"
                                                id={`${route.routeId}-switch`}
                                                className='float-end'
                                                checked={route.enabled} 
                                                label={route.enabled ? "Enabled" : "Disabled"}
                                                onChange={async () => {
                                                    if (route.routeId === undefined || route.enabled === undefined)
                                                        return;
                                                    if (!route.enabled){
                                                        await api.enableRouteAsync(route.routeId);
                                                        setRoutes(routes.map(x => {
                                                            if (x.routeId !== route.routeId)
                                                                return x;
                                                            return {...x, enabled: true}
                                                        }))
                                                    } 
                                                    else if(route.enabled){
                                                        await api.disableRouteAsync(route.routeId);
                                                        setRoutes(routes.map(x => {
                                                            if (x.routeId !== route.routeId)
                                                                return x;
                                                            return {...x, enabled: false}
                                                        }))
                                                    }
                                                }}/>
                                        </Col>
                                    </Row>
                                </Card.Header>
                                <Card.Body>
                                    <Card.Subtitle className='mt-2'>
                                        Mock Response:
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
                                        <Button size='sm' variant='secondary' onClick={() => handleShowDeleteModal(route)} >
                                            <Trash />
                                        </Button>
                                        <Button size='sm' onClick={() => handleShowEditModal(route)}>
                                            <PencilSquare />
                                        </Button>
                                    </Stack>
                                </Card.Footer>
                            </Card>
                        )
                    })}
                </Stack>
            </>
            :
            <>
                <div className="d-flex justify-content-around"><p>{errorMessage}</p></div>
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
                <Button variant="primary" onClick={handleDeleteRoute} disabled={!selectedRoute?.routeId}>
                    Delete
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
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setNewRoute({ ...newRoute, path: e.currentTarget.value })} />
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
                    !newRoute?.method || !newRoute.path || !newRoute.mock
                }>
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
                                <option value="Get">Get</option>
                                <option value="Post">Post</option>
                                <option value="Put">Put</option>
                                <option value="Delete">Delete</option>
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
    </>;
}