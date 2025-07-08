import { create } from 'zustand'
import * as api from '../network/api';
import axios from 'axios';

export interface AsyncDataState<T> {
    isLoading: boolean;
    isError: boolean;
    errorMessage?: string;
    value?: T;
}

export interface StatusCodeSelection {
    [statusCode: string]: string;
}

export interface ServerConfig {
    connectionString: string;
    upstreamUrl: string;
    host: string;
    port: string;
}

export interface MockRoute {
    enabled?: boolean;
    routeId?: string;
    method?: string;
    httpStatusCode?: number;
    path?: string;
    mock?: object;
}

export interface OpenApiDocument {
    documentId: string;
    title?: string;
    version?: string;
    description?: string;
    openApiVersion?: string;
    basePath?: string;
    host?: string;
    schemes?: string[];
    consumes?: string[];
    produces?: string[];
    createdAt: string;
    updatedAt: string;
    isActive: boolean;
}

export interface OpenApiDocumentDetail extends OpenApiDocument {
    originalJson?: string;
    // Note: Complex nested data (paths, tags, servers) is stored as raw BSON
    // and can be parsed from originalJson if needed for display
}

export interface CreateOpenApiDocument {
    title?: string;
    version?: string;
    description?: string;
    openApiJson: string;
}

interface IProckState {
    httpContentTypes: string[],
    getHttpContentTypes: () => void;
    httpStatusCodes: StatusCodeSelection[],
    getHttpStatusCodes: () => void;
    mockRoutes: AsyncDataState<MockRoute[]>,
    getMockRoutes: () => void;
    createMockRoute: (mockRoute: MockRoute) => void;
    updateMockRoute: (mockRoute: MockRoute) => void;
    deleteMockRoute: (mockRouteId: string) => void;
    prockConfig: AsyncDataState<ServerConfig>;
    getProckConfigs: () => Promise<void>;
    updateUpstreamUrl: (upstreamUrl: string) => void;
    openApiDocuments: AsyncDataState<OpenApiDocument[]>;
    getOpenApiDocuments: () => void;
    createOpenApiDocument: (document: CreateOpenApiDocument) => void;
    updateOpenApiDocument: (documentId: string, document: Partial<OpenApiDocument>) => void;
    deleteOpenApiDocument: (documentId: string) => void;
}


export const useProckStore = create<IProckState>()((set, get) => (
    {
        httpContentTypes: [],
        getHttpContentTypes: async () => {
            try {
                const response = await api.fetchHttpContentTypesAsync();
                set({ httpContentTypes: response.data });
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    console.error(error.message);
                } else {
                    console.error(error);
                }
            }
        },
        httpStatusCodes: [],
        getHttpStatusCodes: async () => {
            try {
                const response = await api.fetchHttpStatusCodesAsync();
                set({ httpStatusCodes: response.data });
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    console.error(error.message);
                } else {
                    console.error(error);
                }
            }
        },
        mockRoutes: { isLoading: false, isError: false },
        getMockRoutes: async () => {
            set({ mockRoutes: { isLoading: true, isError: false, value: undefined } });
            try {
                const response = await api.fetchRoutesAsync();
                set({ mockRoutes: { isLoading: false, isError: false, value: response.data } });
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ mockRoutes: { isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ mockRoutes: { isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        createMockRoute: async (mockRoute) => {
            set({ mockRoutes: { isLoading: true, isError: false } });
            const prevMockRoutes = get().mockRoutes.value;
            try {
                const response = await api.createNewRouteAsync(mockRoute);
                if (prevMockRoutes !== undefined) {
                    set({ mockRoutes: { isLoading: false, isError: false, value: [...prevMockRoutes as MockRoute[], response.data] } });
                } else {
                    set({ mockRoutes: { isLoading: false, isError: false, value: [response.data] } });
                }

            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        updateMockRoute: async (mockRoute) => {
            set({ mockRoutes: { isLoading: true, isError: false } });
            const prevMockRoutes = get().mockRoutes.value;
            try {
                const response = await api.updateRouteAsync(mockRoute);
                if (prevMockRoutes !== undefined) {
                    set({
                        mockRoutes: {
                            isLoading: false, isError: false, value: (prevMockRoutes as MockRoute[]).map((x) => {
                                if (x.routeId !== response.data.routeId)
                                    return x;
                                return response.data;
                            })
                        }
                    });
                } 
                else {
                    set({ mockRoutes: { isLoading: false, isError: false, value: [response.data] } });
                }
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } 
                else {
                    const typedError = error as Error;
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        deleteMockRoute: async (mockRouteId) => {
            set({ mockRoutes: { isLoading: true, isError: false } });
            const prevMockRoutes = get().mockRoutes.value;
            try {
                const response = await api.deleteRouteAsync(mockRouteId);
                if (prevMockRoutes !== undefined) {
                    set({
                        mockRoutes: {
                            isLoading: false, isError: false, value: (prevMockRoutes as MockRoute[]).filter((x) => x.routeId !== response.data.routeId)
                        }
                    });
                } 
                else {
                    set({ mockRoutes: { isLoading: false, isError: false, value: undefined } });//todo
                }
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } 
                else {
                    const typedError = error as Error;
                    set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        prockConfig: { isLoading: false, isError: false },
        getProckConfigs: async () => {
            set({ prockConfig: { isLoading: true, isError: false, value: undefined } });
            try {
                const response = await api.fetchServerConfigAsync();
                set({ prockConfig: { isLoading: false, isError: false, value: response.data } });
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ prockConfig: { isLoading: false, isError: true, errorMessage: error.message, value: undefined } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ prockConfig: { isLoading: false, isError: true, errorMessage: typedError.message, value: undefined } });
                }
            }
        },
        updateUpstreamUrl: async (upstreamUrl: string) => {
            const currentConfig = get().prockConfig.value;
            if (currentConfig) {
                const updatedConfig: ServerConfig = { ...currentConfig, upstreamUrl };
                set({ prockConfig: { isLoading: true, isError: false, value: updatedConfig } });
                try {
                    await api.updateUpstreamUrlAsync(upstreamUrl);
                    set({ prockConfig: { isLoading: false, isError: false, value: updatedConfig } });
                } catch (error: unknown) {
                    if (axios.isAxiosError(error)) {
                        set({ prockConfig: { isLoading: false, isError: true, errorMessage: error.message, value: currentConfig } });
                        console.error(error.message);
                    } else {
                        const typedError = error as Error;
                        set({ prockConfig: { isLoading: false, isError: true, errorMessage: typedError.message, value: currentConfig } });
                    }
                }
            }
        },
        openApiDocuments: { isLoading: false, isError: false },
        getOpenApiDocuments: async () => {
            set({ openApiDocuments: { isLoading: true, isError: false, value: undefined } });
            try {
                const response = await api.fetchOpenApiDocumentsAsync();
                set({ openApiDocuments: { isLoading: false, isError: false, value: response.data } });
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ openApiDocuments: { isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ openApiDocuments: { isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        createOpenApiDocument: async (document) => {
            set({ openApiDocuments: { isLoading: true, isError: false } });
            const prevDocuments = get().openApiDocuments.value;
            try {
                const response = await api.createOpenApiDocumentAsync(document);
                if (prevDocuments !== undefined) {
                    set({ openApiDocuments: { isLoading: false, isError: false, value: [...prevDocuments as OpenApiDocument[], response.data] } });
                } else {
                    set({ openApiDocuments: { isLoading: false, isError: false, value: [response.data] } });
                }
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        updateOpenApiDocument: async (documentId: string, document: Partial<OpenApiDocument>) => {
            set({ openApiDocuments: { isLoading: true, isError: false } });
            const prevDocuments = get().openApiDocuments.value;
            try {
                const response = await api.updateOpenApiDocumentAsync(documentId, document);
                if (prevDocuments !== undefined) {
                    const updatedDocuments = prevDocuments.map(doc => 
                        doc.documentId === documentId ? response.data : doc
                    );
                    set({ openApiDocuments: { isLoading: false, isError: false, value: updatedDocuments } });
                } else {
                    set({ openApiDocuments: { isLoading: false, isError: false, value: [response.data] } });
                }
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        },
        deleteOpenApiDocument: async (documentId: string) => {
            set({ openApiDocuments: { isLoading: true, isError: false } });
            const prevDocuments = get().openApiDocuments.value;
            try {
                await api.deleteOpenApiDocumentAsync(documentId);
                if (prevDocuments !== undefined) {
                    set({
                        openApiDocuments: {
                            isLoading: false, isError: false, value: (prevDocuments as OpenApiDocument[]).filter((x) => x.documentId !== documentId)
                        }
                    });
                } else {
                    set({ openApiDocuments: { isLoading: false, isError: false, value: undefined } });
                }
            }
            catch (error: unknown) {
                if (axios.isAxiosError(error)) {
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: error.message } });
                    console.error(error.message);
                } else {
                    const typedError = error as Error;
                    set({ openApiDocuments: { ...get().openApiDocuments, isLoading: false, isError: true, errorMessage: typedError.message } });
                }
            }
        }
    }
)
)
