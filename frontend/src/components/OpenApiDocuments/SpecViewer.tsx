
import { useEffect, useState } from 'react';
import * as api from '../../network/api';
import { OpenAPI } from '@scalar/openapi-types';
import { JsonView, allExpanded, darkStyles } from 'react-json-view-lite';
import 'react-json-view-lite/dist/index.css';
import { Container } from 'react-bootstrap';
const SpecViewer = ({ documentId }: {
    documentId: string;
}) => {
    const [spec, setSpec] = useState<OpenAPI.Document>();

    useEffect(() => {
        api.fetchOpenApiDocumentJsonAsync(documentId)
            .then(response => {
                if (response.status === 200) {
                    setSpec({ ...response.data, openapi: response.data.openapi || '3.0.0' } as OpenAPI.Document);
                } else {
                    throw new Error(`Failed to fetch OpenAPI document: ${response.statusText}`);
                }
            })
            .catch(error => {
                console.error('Error fetching OpenAPI document:', error);
                setSpec(undefined); // Clear spec on error
            });
    }, [documentId]);


    return (
        <Container id="json-container">
            <JsonView data={spec || ""} shouldExpandNode={allExpanded} style={darkStyles} />
        </Container>
    );
};
export default SpecViewer;