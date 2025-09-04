import axios from "axios";
import { MockRoute } from "../store/useProckStore";
import { OpenAPI } from "@scalar/openapi-types";

axios.interceptors.request.use(
    config => {
        config.headers['Accept'] = 'application/json';
        config.headers['Content-Type'] = 'application/json';
            return config;
        },
    error => {
        if (axios.isAxiosError(error)) {
            console.error(error.message);
        } else {
            console.error(error);
        }
        return Promise.reject(error);
    }
);

export const restartAsync = () => {
    return axios.post("/prock/api/restart");
}
export const fetchRoutesAsync = () => {
    return axios.get("/prock/api/mock-routes");
}
export const fetchServerConfigAsync = () => {
    return axios.get("/prock/api/config");
}
export const fetchHttpStatusCodesAsync = () => {
    return axios.get("/prock/api/http-status-codes");
}
export const fetchHttpContentTypesAsync = () => {
    return axios.get("/prock/api/http-content-types");
}
export const createNewRouteAsync = (newRoute: MockRoute) => {
    return axios.post("/prock/api/mock-routes", JSON.stringify(newRoute));
}
export const updateRouteAsync = (routeId: number, newRoute: MockRoute) => {
    return axios.put<MockRoute>(`/prock/api/mock-routes/${routeId}`, JSON.stringify(newRoute));
}
export const deleteRouteAsync = (routeId: number) => {
    return axios.delete(`/prock/api/mock-routes/${routeId}`);
}
export const enableRouteAsync = (routeId: number) => {
    return axios.put(`/prock/api/mock-routes/${routeId}/enable`);
}
export const disableRouteAsync = (routeId: number) => {
    return axios.put(`/prock/api/mock-routes/${routeId}/disable`);
}

export function updateUpstreamUrlAsync(updatedUrl: string) {
    return axios.put("/prock/api/config/upstream-url", { upstreamUrl: updatedUrl });
}

// OpenAPI Document API functions
export const fetchOpenApiDocumentsAsync = () => {
    return axios.get("/prock/api/openapi/documents");
}

export const fetchOpenApiDocumentByIdAsync = (documentId: number) => {
    return axios.get(`/prock/api/openapi/documents/${documentId}`);
}

export const createOpenApiDocumentAsync = (document: {
    title?: string;
    version?: string;
    description?: string;
    originalJson: string;
}) => {
    return axios.post("/prock/api/openapi/documents", JSON.stringify(document));
}

export const updateOpenApiDocumentAsync = (documentId: number, document: {
    title?: string;
    version?: string;
    description?: string;
    originalJson?: string;
    isActive?: boolean;
}) => {
    return axios.put(`/prock/api/openapi/documents/${documentId}`, JSON.stringify(document));
}

export const deleteOpenApiDocumentAsync = (documentId: number) => {
    return axios.delete(`/prock/api/openapi/documents/${documentId}`);
}

export const fetchOpenApiDocumentJsonAsync = (documentId: number) => {
    return axios.get<OpenAPI.Document>(`/prock/api/openapi/documents/${documentId}/json`);
}

export const generateMockRoutesFromOpenApi = (documentId: number) => {
    return axios.post(`/prock/api/openapi/documents/${documentId}/generate-mocks`);
};