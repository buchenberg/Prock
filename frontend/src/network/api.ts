import axios from "axios";
import { IMockRoute } from "../components/MockRoutes";

axios.interceptors.request.use(
    config => {
        config.headers['Accept'] = 'application/json';
        config.headers['Content-Type'] = 'application/json';
            return config;
        },
    error => {
        console.error("Error intercepted", error)
        return Promise.reject(error);
    }
);

export const restartAsync = () => {
    return axios.post("/prock/api/restart");
}

export const fetchRoutesAsync = () => {
    return axios("/prock/api/mock-routes");
}

export const fetchServerConfigAsync = () => {
    return axios("/prock/api/config");
}

export const createNewRouteAsync = (newRoute: IMockRoute) => {
    return axios.post("/prock/api/mock-routes", JSON.stringify(newRoute));
}

export const updateRouteAsync = (newRoute: IMockRoute) => {
    return axios.put("/prock/api/mock-routes", JSON.stringify(newRoute));
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