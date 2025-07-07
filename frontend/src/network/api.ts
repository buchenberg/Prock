import axios from "axios";
import { MockRoute } from "../store/store";

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