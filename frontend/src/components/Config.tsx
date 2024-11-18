
import './MockRoutes.css';
import { useEffect } from "react";
import { Alert, Card, Col, Container, Row, Spinner } from "react-bootstrap";
import { AsyncDataState, ServerConfig, useProckStore } from '../store/store';



export default function Config() {
    const prockConfig: AsyncDataState<ServerConfig> = useProckStore((state) => state.prockConfig);
    const getProckConfigs = useProckStore((state) => state.getProckConfigs);

    useEffect(() => {
        if (prockConfig.value == undefined && !prockConfig.isLoading && !prockConfig.isError) {
            getProckConfigs();
        }
    }, [getProckConfigs, prockConfig.isError, prockConfig.isLoading, prockConfig.value]);



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
                        <Col><b>Upstream URL</b></Col>
                        <Col>{prockConfig.value.upstreamUrl ?? ""}</Col>
                    </Row>
                    <hr />
                    <Row>
                        <Col><b>MongoDb Connection String</b></Col>
                        <Col>{prockConfig.value.connectionString ?? ""}</Col>
                    </Row>
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