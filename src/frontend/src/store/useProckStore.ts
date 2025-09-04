import { create } from 'zustand'
import * as api from '../network/api';
import axios from 'axios';
import { AsyncData } from './AsyncData';
import { generateMockRoutesFromOpenApi } from '../network/api';

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
    id?: number;
    routeId?: string;
    enabled?: boolean;
    method?: string;
    httpStatusCode?: number;
    path?: string;
    mock?: object;
}

interface ProckStore {
    httpContentTypes: string[],
    getHttpContentTypes: () => void;
    httpStatusCodes: StatusCodeSelection[],
    getHttpStatusCodes: () => void;
    mockRoutes: AsyncData<MockRoute[]>,
    getMockRoutes: () => void;
    createMockRoute: (mockRoute: MockRoute) => void;
    updateMockRoute: (mockRoute: MockRoute) => void;
    deleteMockRoute: (mockRouteId: number) => void;
    prockConfig: AsyncData<ServerConfig>;
    getProckConfigs: () => Promise<void>;
    updateUpstreamUrl: (upstreamUrl: string) => void;
    generateMockRoutesFromOpenApi: (documentId: number) => void;
}

export const useProckStore = create<ProckStore>()((set, get) => ({
    httpContentTypes: [],
    getHttpContentTypes: async () => {
        try {
            const response = await api.fetchHttpContentTypesAsync();
            set({ httpContentTypes: response.data });
        } catch (error) {
            console.error("Error fetching HTTP content types:", error);
        }
    },
    httpStatusCodes: [],
    getHttpStatusCodes: async () => {
        try {
            const response = await api.fetchHttpStatusCodesAsync();
            set({ httpStatusCodes: response.data });
        } catch (error) {
            console.error("Error fetching HTTP status codes:", error);
        }
    },
    mockRoutes: { isLoading: false, isError: false },
    getMockRoutes: async () => {
        set({ mockRoutes: { isLoading: true, isError: false } });
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
    createMockRoute: async (mockRoute: MockRoute) => {
        set({ mockRoutes: { isLoading: true, isError: false } });
        const prevMockRoutes = get().mockRoutes.value;
        try {
            const response = await api.createNewRouteAsync(mockRoute);
            if (prevMockRoutes !== undefined) {
                set({ mockRoutes: { isLoading: false, isError: false, value: [...prevMockRoutes, response.data] } });
            } else {
                set({ mockRoutes: { isLoading: false, isError: false, value: [response.data] } });
            }
        } catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                const errorMessage = error.response?.data?.detail || error.response?.data?.title || error.message;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage } });
                console.error(errorMessage);
                throw new Error(errorMessage); // Re-throw for UI to handle
            } else {
                const typedError = error as Error;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
                throw typedError; // Re-throw for UI to handle
            }
        }
    },
    updateMockRoute: async (mockRoute: MockRoute) => {
        set({ mockRoutes: { isLoading: true, isError: false } });
        const prevMockRoutes = get().mockRoutes.value;
        try {
            if (!mockRoute.id) {
                throw new Error('MockRoute ID is required for updates');
            }
            const response = await api.updateRouteAsync(mockRoute.id, mockRoute);
            if (prevMockRoutes !== undefined) {
                const updatedMockRoutes = prevMockRoutes.map((r) =>
                    r.id === mockRoute.id ? response.data : r
                );
                set({ mockRoutes: { isLoading: false, isError: false, value: updatedMockRoutes } });
            } else {
                set({ mockRoutes: { isLoading: false, isError: false, value: [response.data] } });
            }
        } catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    },
    deleteMockRoute: async (mockRouteId: number) => {
        set({ mockRoutes: { isLoading: true, isError: false } });
        try {
            await api.deleteRouteAsync(mockRouteId);
            // Refetch the data from server to ensure consistency
            await get().getMockRoutes();
        } catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
            }
        }
    },
    prockConfig: { isLoading: false, isError: false },
    getProckConfigs: async () => {
        set({ prockConfig: { isLoading: true, isError: false } });
        try {
            const response = await api.fetchServerConfigAsync();
            set({ prockConfig: { isLoading: false, isError: false, value: response.data } });
        }
        catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                set({ prockConfig: { isLoading: false, isError: true, errorMessage: error.message } });
                console.error(error.message);
            } else {
                const typedError = error as Error;
                set({ prockConfig: { isLoading: false, isError: true, errorMessage: typedError.message } });
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
    generateMockRoutesFromOpenApi: async (documentId: number) => {
        set({ mockRoutes: { ...get().mockRoutes, isLoading: true } });
        try {
            const response = await generateMockRoutesFromOpenApi(documentId);
            // Merge with existing routes instead of replacing
            const currentRoutes = get().mockRoutes.value || [];
            const newRoutes = [...currentRoutes, ...response.data];
            set({ mockRoutes: { isLoading: false, isError: false, value: newRoutes } });
        } catch (error: unknown) {
            if (axios.isAxiosError(error)) {
                const errorMessage = error.response?.data?.detail || error.response?.data?.title || error.message;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage } });
                console.error(errorMessage);
                throw new Error(errorMessage); // Re-throw for UI to handle
            } else {
                const typedError = error as Error;
                set({ mockRoutes: { ...get().mockRoutes, isLoading: false, isError: true, errorMessage: typedError.message } });
                throw typedError; // Re-throw for UI to handle
            }
        }
    },
}))
