import { useState, useEffect } from "react";
import { Card, Container, ListGroup, Stack } from "react-bootstrap";
import Editor, { DiffEditor, useMonaco, loader } from '@monaco-editor/react';


export interface IMockRoute {
    routeId: string;
    method: string;
    path: string;
    mock: object;
}

export default function MockRoutes() {

    const [routes, setRoutes] = useState<IMockRoute[]>();

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


    return <Container>{routes &&
        <div>
            <h2>Routes</h2>
            <Stack gap={3}>
                {routes.map((route) => {
                    return (
                        <Card key={route.routeId}>
                            <Card.Header>
                                <Card.Title><b>{route.method.toUpperCase()}</b> {route.path}</Card.Title>
                            </Card.Header>
                            <Card.Body>
                                Mock
                                <Card body>
                                    <pre>
                                        <code>
                                            {JSON.stringify(route.mock, null, 2)}
                                        </code>
                                    </pre>
                                </Card>

                                {/* <Editor
                                    className="p-3"
                                    theme="vs-dark"
                                    height={"100px"}
                                    defaultLanguage="json"
                                    //defaultValue={JSON.stringify(route.mock, null, 2)} 
                                    value={JSON.stringify(route.mock, null, 2)} /> */}
                            </Card.Body>
                        </Card>
                    )

                })}
            </Stack>
        </div>

    }</Container>;
}