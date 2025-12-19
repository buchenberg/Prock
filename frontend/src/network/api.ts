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
export const updateRouteAsync = (newRoute: MockRoute) => {
    return axios.put<MockRoute>("/prock/api/mock-routes", JSON.stringify(newRoute));
}
export const deleteRouteAsync = (routeId: string) => {
    return axios.delete(`/prock/api/mock-routes/${routeId}`);
}
export const enableRouteAsync = (routeId: string) => {
    return axios.put(`/prock/api/mock-routes/${routeId}/enable-route`);
}
export const disableRouteAsync = (routeId: string) => {
    return axios.put(`/prock/api/mock-routes/${routeId}/disable-route`);
}
export const deleteAllRoutesAsync = () => {
    return axios.delete<number>("/prock/api/mock-routes");
}

export function updateUpstreamUrlAsync(updatedUrl: string) {
    return axios.put("/prock/api/config/upstream-url", { upstreamUrl: updatedUrl });
}

// OpenAPI Document API functions
export const fetchOpenApiDocumentsAsync = () => {
    return axios.get("/prock/api/openapi-documents");
}

export const fetchOpenApiDocumentByIdAsync = (documentId: string) => {
    return axios.get(`/prock/api/openapi-documents/${documentId}`);
}

export const createOpenApiDocumentAsync = (document: {
    title?: string;
    version?: string;
    description?: string;
    openApiJson: string;
}) => {
    return axios.post("/prock/api/openapi-documents", JSON.stringify(document));
}

export const updateOpenApiDocumentAsync = (documentId: string, document: {
    title?: string;
    version?: string;
    description?: string;
    openApiJson?: string;
    isActive?: boolean;
}) => {
    return axios.put(`/prock/api/openapi-documents/${documentId}`, JSON.stringify(document));
}

export const deleteOpenApiDocumentAsync = (documentId: string) => {
    return axios.delete(`/prock/api/openapi-documents/${documentId}`);
}

export const fetchOpenApiDocumentJsonAsync = (documentId: string) => {
    return axios.get<OpenAPI.Document | string>(`/prock/api/openapi-documents/${documentId}/json`);
}

export const generateMockRoutesFromOpenApi = (documentId: string) => {
    return axios.post(`/prock/api/openapi-documents/${documentId}/generate-mock-routes`);
};