import { create } from 'zustand'
import * as api from '../network/api';
import axios from 'axios';
import { OpenAPI } from '@scalar/openapi-types';
import { AsyncData } from './AsyncData';

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
    // value can be the parsed object (OpenAPI.Document) or a raw YAML string
    value?: OpenAPI.Document | string;
}

export interface CreateOpenApiDocument {
    title?: string;
    version?: string;
    description?: string;
    openApiJson: string;
}

interface OpenApiStore {
    documents: AsyncData<OpenApiDocument[]>;
    documentDetail: AsyncData<OpenAPI.Document | string | undefined>;
    getDocuments: () => void;
    createDocument: (document: CreateOpenApiDocument) => void;
    updateDocument: (documentId: string, document: Partial<OpenApiDocument>) => void;
    deleteDocument: (documentId: string) => void;
    fetchOpenApiJson: (documentId: string) => void;
}

export const useOpenApiStore = create<OpenApiStore>()((set, get) => ({
    documents: { isLoading: false, isError: false },
    documentDetail: { isLoading: false, isError: false },
    getDocuments: async () => {
        set({ documents: { isLoading: true, isError: false, value: undefined } });
        try {
            const response = await api.fetchOpenApiDocumentsAsync();
            set({ documents: { isLoading: false, isError: false, value: response.data } });
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ documents: { isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ documents: { isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    },
    createDocument: async (document) => {
        const prevDocuments = get().documents.value;
        set({ documents: { ...get().documents, isLoading: true, isError: false } });
        try {
            const response = await api.createOpenApiDocumentAsync(document);
            if (prevDocuments !== undefined) {
                set({ documents: { isLoading: false, isError: false, value: [...prevDocuments as OpenApiDocument[], response.data] } });
            } else {
                set({ documents: { isLoading: false, isError: false, value: [response.data] } });
            }
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                let message: string;

                switch (error.status) {
                    case 400:
                        message = error.response?.data || "Invalid request";
                        break;
                    case 500:
                        message = "There was an issue fulfilling your request. Please try again later.";
                        break;
                    default:
                        message = error.message
                }
                set({ documents: { isLoading: false, isError: true, errorMessage: message, value: prevDocuments } });

            } else {
                const typedError = error as Error;
                set({ documents: { isLoading: false, isError: true, errorMessage: typedError.message, value: prevDocuments } });
            }
        }
    },
    updateDocument: async (documentId: string, document: Partial<OpenApiDocument>) => {
        const prevDocuments = get().documents.value;
        set({ documents: { ...get().documents, isLoading: true, isError: false } });
        try {
            const response = await api.updateOpenApiDocumentAsync(documentId, document);
            if (prevDocuments !== undefined) {
                const updatedDocuments = prevDocuments.map(doc =>
                    doc.documentId === documentId ? response.data : doc
                );
                set({ documents: { isLoading: false, isError: false, value: updatedDocuments } });
            } else {
                set({ documents: { isLoading: false, isError: false, value: [response.data] } });
            }
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ documents: { ...get().documents, isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ documents: { ...get().documents, isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    },
    fetchOpenApiJson: async (documentId: string) => {
        set({ documentDetail: { isLoading: true, isError: false } });
        try {
            const response = await api.fetchOpenApiDocumentJsonAsync(documentId);
            set({ documentDetail: { isLoading: false, isError: false, value: response.data } });
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ documentDetail: { isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ documentDetail: { isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    },
    deleteDocument: async (documentId: string) => {
        const prevDocuments = get().documents.value;
        set({ documents: { ...get().documents, isLoading: true, isError: false } });
        try {
            await api.deleteOpenApiDocumentAsync(documentId);
            if (prevDocuments !== undefined) {
                set({
                    documents: {
                        isLoading: false, isError: false, value: (prevDocuments as OpenApiDocument[]).filter((x) => x.documentId !== documentId)
                    }
                });
            } else {
                set({ documents: { isLoading: false, isError: false, value: undefined } });
            }
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ documents: { ...get().documents, isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ documents: { ...get().documents, isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    }
}));
