import { Container, Card, Col, Row } from "react-bootstrap";

const Home = () => (
    <Container className='mt-3' fluid>
        <h4>👋 Welcome to Prock, Doc!</h4>
        <p className="text-muted">
            <b>Prock Mocking Proxy</b> is your ninja sidekick for mocking and proxying API requests. Use it to speed up UI development, test those weird edge cases, or just keep building even when the backend isn’t ready yet.
        </p>
        <Card className='mb-3'>
            <Card.Body>
                <h5>Getting Started</h5>
                <p className="text-muted">
                    Here&apos;s how to get the most out of Prock. Just use the tabs below:
                </p>
                <Row>
                    <Col>
                        <h6>Config</h6>
                        <p className="text-muted">
                            Set or update the upstream API URL in the <b>Config</b> tab. If there&apos;s no mock for a route, Prock will forward your request there.
                        </p>
                    </Col>
                    <Col>
                        <h6>OpenAPI</h6>
                        <p className="text-muted">
                            Upload your OpenAPI (Swagger) spec in the <b>OpenAPI</b> tab. With one click, you can auto-generate mock routes for every path in your API!
                        </p>
                    </Col>
                    <Col>
                        <h6>Mocks</h6>
                        <p className="text-muted">
                            Head to the <b>Mocks</b> tab to create, edit, or delete mock routes. If a request matches a mock, you&apos;ll get your custom response—no backend needed.
                        </p>
                    </Col>
                    <Col>
                        <h6>Logs</h6>
                        <p className="text-muted">
                            The <b>Logs</b> tab shows you what&apos;s been happening lately—see all the requests Prock has handled.
                        </p>
                    </Col>
                </Row>
            </Card.Body>
        </Card>
    </Container>
);
export default Home;