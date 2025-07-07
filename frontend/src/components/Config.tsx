
import './MockRoutes.css';
import { useEffect, useState } from "react";
import { Alert, Button, Card, Col, Container, Form, Row, Spinner, Stack } from "react-bootstrap";
import { AsyncDataState, ServerConfig, useProckStore } from '../store/store';
import { PencilSquare } from 'react-bootstrap-icons';



export default function Config() {
    const prockConfig: AsyncDataState<ServerConfig> = useProckStore((state) => state.prockConfig);
    const getProckConfigs = useProckStore((state) => state.getProckConfigs);
    const updateUpstreamUrl = useProckStore((state) => state.updateUpstreamUrl);
    const [upstreamUrl, setUpstreamUrl] = useState<string>("");
    const [showEdit, setShowEdit] = useState<boolean>(false);

    useEffect(() => {
        async function fetchConfigs() {
            if (prockConfig.value == undefined && !prockConfig.isLoading && !prockConfig.isError) {
                await getProckConfigs();
            }
        }
        fetchConfigs();
    }, [getProckConfigs, prockConfig.isError, prockConfig.isLoading, prockConfig.value]);

    useEffect(() => {
        if (prockConfig.value) {
            setUpstreamUrl(prockConfig.value.upstreamUrl ?? "");
        }
    }, [prockConfig.value]);


    const handleShowEditUrl = () => {
        setShowEdit(prev => !prev);
    };



    return <>
        {prockConfig.value ?
            <Container fluid className='mt-3'>
                <div className='mb-3'>
                    <h4>Configuration</h4>
                </div>
                <Card body>
                    <Row>
                        <Col><b>Host</b></Col>
                        <Col>{prockConfig.value.host ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>Port</b></Col>
                        <Col>{prockConfig.value.port ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>MongoDb Connection String</b></Col>
                        <Col>{prockConfig.value.connectionString ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>Upstream URL</b></Col>
                        <Col>
                            <Stack direction='horizontal' gap={2}>
                                <span>{prockConfig.value.upstreamUrl ?? ""}</span>
                                <PencilSquare
                                    className="icon-btn text-primary"
                                    onClick={handleShowEditUrl}
                                />
                            </Stack>
                        </Col>
                    </Row>
                    {showEdit &&
                        <Row>
                            <Form.Group as={Col} className='mt-3'>
                                <Form.Control
                                    type="text"
                                    value={upstreamUrl ?? ""}
                                    onChange={(e) => {
                                        setUpstreamUrl(e.target.value);
                                    }}
                                />
                                <Button
                                    className='mt-2'
                                    variant='primary'
                                    disabled={prockConfig.isLoading || upstreamUrl === prockConfig.value?.upstreamUrl}
                                    onClick={() => {
                                        updateUpstreamUrl(upstreamUrl);
                                    }}
                                >
                                    {prockConfig.isLoading ? <Spinner animation="border" size="sm" /> : "Update"}
                                </Button>
                            </Form.Group>
                        </Row>
                    }
                </Card>
            </Container>
            :
            <Container className='mt-3'>
                {prockConfig.isLoading ?
                    <div className="d-flex justify-content-around"><Spinner className='m-4 text-center' variant='warning' /></div>
                    :
                    <div className="d-flex justify-content-around"><Alert className='m-4 text-center' variant='warning' />An error encountered trying to load configuration data. {prockConfig.errorMessage}</div>
                }

            </Container>
        }
    </>;
}