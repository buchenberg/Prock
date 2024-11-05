import { Editor } from '@monaco-editor/react';
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Button, Card, Col, Container, Form, Modal, Row, Stack } from "react-bootstrap";
import { PencilSquare, PlusCircle } from "react-bootstrap-icons";


export interface IMockRoute {
    routeId: string;
    method: string;
    path: string;
    mock: object;
}

export default function MockRoutes() {

    const [routes, setRoutes] = useState<IMockRoute[]>();
    const [selectedRoute, setSelectedRoute] = useState<IMockRoute>();


    const [show, setShow] = useState(false);

    const handleClose = () => {
        setSelectedRoute(undefined);
        setShow(false);
    }
    const handleShow = (route: IMockRoute) => {
        setSelectedRoute(route);
        setShow(true);
    }

    const [showCreateModal, setShowCreateModal] = useState(false);

    const handleCloseCreateModal = () => {
        setShowCreateModal(false);
    }
    const handleShowCreateModal = () => {
        setShowCreateModal(true);
    }

    async function getData() {
        const url = "/drunken-master/api/mock-routes";
        try {
            const response = await fetch(url, { mode: 'cors' });
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            const json = await response.json();
            setRoutes(json);
        } catch (error) {
            console.error(error);
        }
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
                                            <PencilSquare className="float-end icon-btn" onClick={() => handleShow(route)} />
                                        </Col>
                                    </Row>
                                </Card.Header>
                                <Card.Body>
                                    <Card.Subtitle>
                                        Method: {route.method.toUpperCase()}
                                    </Card.Subtitle>
                                    <Card.Subtitle className='mt-2'>
                                        Response:
                                    </Card.Subtitle>
                                    <Card.Text >
                                        <Card body className='mt-2'>
                                            <pre>
                                                <code>
                                                    {JSON.stringify(route.mock, null, 2)}
                                                </code>
                                            </pre>
                                        </Card>
                                    </Card.Text >


                                </Card.Body>
                            </Card>
                        )
                    })}
                </Stack>
            </div>

        }
        <Modal show={show} onHide={handleClose}>
            <Modal.Header closeButton>
                <Modal.Title>Edit Mock Route</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {selectedRoute &&
                    <Container>
                        <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                            <Form.Label>Method</Form.Label>
                            <Form.Label>HTTP Method</Form.Label>
                            <Form.Select value={selectedRoute.method}>
                                <option value={""}>Select...</option>
                                <option value="Get">Get</option>
                                <option value="Post">Post</option>
                                <option value="Put">Put</option>
                                <option value="Delete">Delete</option>
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                            <Form.Label>Path</Form.Label>
                            <Form.Control type="text" placeholder="/some/path" value={selectedRoute.path} />
                        </Form.Group>
                        <Editor
                            theme="vs-dark"
                            height={"100px"}
                            defaultLanguage="json"
                            defaultValue={"{}"}
                            value={JSON.stringify(selectedRoute.mock, null, 2)} />
                    </Container>
                }

            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleClose}>
                    Cancel
                </Button>
                <Button variant="primary" onClick={handleClose}>
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
        <Modal show={showCreateModal} onHide={handleCloseCreateModal}>
            <Modal.Header closeButton>
                <Modal.Title>Add Route</Modal.Title>
            </Modal.Header>
            <Modal.Body>

                <Container>
                    <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                        <Form.Label>HTTP Method</Form.Label>
                        <Form.Select>
                            <option value={""}>Select...</option>
                            <option value="Get">Get</option>
                            <option value="Post">Post</option>
                            <option value="Put">Put</option>
                            <option value="Delete">Delete</option>
                        </Form.Select>
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                        <Form.Label>Path</Form.Label>
                        <Form.Control type="text" placeholder="/some/path" />
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
                        <Form.Label>Mock Response</Form.Label>
                        <Editor
                            theme="vs-dark"
                            height={"100px"}
                            defaultLanguage="json"
                            defaultValue={""} />
                    </Form.Group>

                </Container>


            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleCloseCreateModal}>
                    Cancel
                </Button>
                <Button variant="primary" onClick={handleCloseCreateModal}>
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
    </>;
}