import { Editor } from '@monaco-editor/react';
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Button, Card, Col, Container, Form, Modal, Row, Stack } from "react-bootstrap";
import { ArrowCounterclockwise, PencilSquare, PlusCircle } from "react-bootstrap-icons";
import { CodeBlock, CopyBlock, dracula, monokai, obsidian, rainbow, zenburn } from "react-code-blocks";


export interface IMockRoute {
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
            console.info(json);
            setRoutes([
                ...(routes as []),
                json
            ]);
            handleCloseCreateModal();
        } catch (error) {
            handleCloseEditModal();
            console.error(error);
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
            console.error(error);
        }
    }

    const handleRestart = async () => {
        const url = "/drunken-master/api/restart";
        try {
            const response = await fetch(url, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const message = await response.json();
            console.info(message);

        } catch (error) {
            handleCloseEditModal();
            console.error(error);
        }
    }

    async function getData() {
        const url = "/drunken-master/api/mock-routes";
        try {
            const response = await fetch(url, { mode: 'cors' });
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const json = await response.json() as IMockRoute[];
            setRoutes(json);
        } catch (error) {
            console.error(error);
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


    useEffect(() => {
        getData()
    }, []);


    return <>
        {routes &&
            <div>
                <Container className='mt-3'>
                    <Stack direction='horizontal' gap={2}>
                        <h4>Mock Routes</h4>
                        <PlusCircle className='icon-btn' onClick={handleShowCreateModal} />
                        {/* <ArrowCounterclockwise className='icon-btn' onClick={handleRestart} /> */}
                    </Stack>
                </Container>
                <Stack gap={3}>
                    {routes.map((route) => {
                        return (
                            <Card key={route.routeId}>
                                <Card.Header>
                                    <Row>
                                        <Col>
                                            <Card.Title>{route.path}</Card.Title>
                                        </Col>
                                        <Col>
                                            <PencilSquare className="float-end icon-btn" onClick={() => handleShowEditModal(route)} />
                                        </Col>
                                    </Row>
                                </Card.Header>
                                <Card.Body>
                                    <Card.Subtitle>
                                        Method: {route.method?.toUpperCase()}
                                    </Card.Subtitle>
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
            </div>

        }
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
                    !newRoute
                    || !newRoute.method
                    || !newRoute.path
                    || !newRoute.mock
                }>
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
    </>;
}