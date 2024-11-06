import { Editor } from '@monaco-editor/react';
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Badge, Button, Card, Col, Container, Form, Modal, Row, Spinner, Stack } from "react-bootstrap";
import { PencilSquare, PlusCircle } from "react-bootstrap-icons";
import { CodeBlock, obsidian } from "react-code-blocks";
import * as api from '../network/api';


export interface IMockRoute {
    routeId?: string;
    method?: string;
    path?: string;
    mock?: object;
}

export interface IServerConfig {
    "connectionString": string;
    "upstreamUrl": string;
    "host": string;
    "port": string;
}

export default function MockRoutes() {

    const [routes, setRoutes] = useState<IMockRoute[]>();
    const [serverConfig, setServerConfig] = useState<IServerConfig>();
    const [selectedRoute, setSelectedRoute] = useState<IMockRoute>();
    const [newRoute, setNewRoute] = useState<IMockRoute>();
    const delay = (ms: number) => new Promise(res => setTimeout(res, ms));
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

    const handleSubmitNewRoute = async () => {
        // Validate mock
        if (!newRoute?.mock) {
            console.error("no new mock to submit!");
            return;
        }


        const url = "/drunken-master/api/mock-routes";
        try {
            const response = await fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newRoute)
            });
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const json = await response.json() as IMockRoute;
            setRoutes([
                ...(routes as []),
                json
            ]);
            handleCloseCreateModal();
        } catch (error) {
            handleCloseCreateModal();
            if (error instanceof Error) {
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
        const url = "/drunken-master/api/mock-routes";
        try {
            const response = await fetch(url, {
                method: 'PUT',
                mode: 'cors',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(selectedRoute)
            });
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const updatedRoute = await response.json() as IMockRoute;
            console.info(updatedRoute);
            setRoutes(routes?.map((route) => (route.routeId === updatedRoute.routeId) ? updatedRoute : route));
            handleCloseEditModal();
        } catch (error) {
            handleCloseEditModal();
            if (error instanceof Error) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }

    const getRoutes = async () => {
        try {
            const response = await api.fetchRoutesAsync();
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const json = await response.json() as IMockRoute[];
            setRoutes(json);
        } catch (error) {
            if (error instanceof Error) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }

    const getServerConfig = async () => {
        try {
            const response = await api.fetchServerConfigAsync();
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }
            const json = await response.json() as IServerConfig;
            setServerConfig(json);
        } catch (error) {
            if (error instanceof Error) {
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
        } catch (error) {
            console.error(error);
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
        if (!serverConfig) {
            getServerConfig();
        }
    }, [routes, serverConfig]);



    return <>
        {routes ?
            <Container className='mt-3'>
                <div className='mb-3'>
                    <Stack direction='horizontal' gap={2} >
                        <h4>Server Configuration</h4>
                    </Stack>
                </div>
                <Card body>
                    <Row>
                        <Col><b>Host</b></Col>
                        <Col>{serverConfig?.host ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>Port</b></Col>
                        <Col>{serverConfig?.port ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>Upstream URL</b></Col>
                        <Col>{serverConfig?.upstreamUrl ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>MongoDb Connection String</b></Col>
                        <Col>{serverConfig?.connectionString ?? ""}</Col>
                    </Row>
                </Card>
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
                                        <Col>
                                            <PencilSquare className="float-end icon-btn" onClick={() => handleShowEditModal(route)} />
                                        </Col>
                                    </Row>
                                </Card.Header>
                                <Card.Body>
                                    <Card.Subtitle className='mt-2'>
                                        Response:
                                    </Card.Subtitle>
                                    <Card.Body>
                                        <CodeBlock
                                            language="json"
                                            text={JSON.stringify(route.mock, null, 2)}
                                            theme={obsidian}
                                            showLineNumbers={false}
                                        />
                                    </Card.Body>
                                </Card.Body>
                            </Card>
                        )
                    })}
                </Stack>
            </Container>
            :
            <Container className='mt-3'>
                <div className="d-flex justify-content-around"><p>{errorMessage}</p></div>
                <div className="d-flex justify-content-around"><Spinner className='m-4 text-center' variant='warning' /></div>
            </Container>
        }
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
                                onChange={(value) => setSelectedRoute({ ...selectedRoute, mock: isJsonString(value) ? JSON.parse(value as string) : selectedRoute.mock })}
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