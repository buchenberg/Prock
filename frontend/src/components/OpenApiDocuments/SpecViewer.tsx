import { useEffect } from 'react';
import { JsonView, allExpanded, darkStyles } from 'react-json-view-lite';
import 'react-json-view-lite/dist/index.css';
import { Container } from 'react-bootstrap';
import { useOpenApiStore } from '../../store/useOpenApiStore';

const SpecViewer = ({ documentId }: {
    documentId: string;
}) => {
    const { documentDetail, fetchOpenApiJson } = useOpenApiStore();

    useEffect(() => {
        if (documentId) {
            fetchOpenApiJson(documentId);
        }
    }, [documentId, fetchOpenApiJson]);

    return (
        <Container id="json-container">
            <JsonView 
                data={documentDetail.value || ""} 
                shouldExpandNode={allExpanded} 
                style={darkStyles} 
            />
        </Container>
    );
};

export default SpecViewer;