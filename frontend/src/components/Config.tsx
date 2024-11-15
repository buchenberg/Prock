
import './MockRoutes.css';
import { useState, useEffect } from "react";
import { Card, Col, Container, Row, Spinner } from "react-bootstrap";
import * as api from '../network/api';
import axios from 'axios';

export interface IServerConfig {
    "connectionString": string;
    "upstreamUrl": string;
    "host": string;
    "port": string;
}

export default function Config() {

    const [serverConfig, setServerConfig] = useState<IServerConfig>();
    const [errorMessage, setErrorMessage] = useState("");

    const getServerConfig = async () => {
        try {
            const response = await api.fetchServerConfigAsync();
            const json = await response.data as IServerConfig;
            setServerConfig(json);
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                setErrorMessage(error.message);
            } else {
                console.error(error);
            }
        }
    }


    useEffect(() => {
        if (!serverConfig) {
            getServerConfig();
        }
    }, [serverConfig]);



    return <>
        {serverConfig ?
            <Container fluid>
                <div className='mb-3'>
                    <h4>Configuration</h4>
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
            </Container>
            :
            <Container className='mt-3'>
                <div className="d-flex justify-content-around"><p>{errorMessage}</p></div>
                <div className="d-flex justify-content-around"><Spinner className='m-4 text-center' variant='warning' /></div>
            </Container>
        }
    </>;
}