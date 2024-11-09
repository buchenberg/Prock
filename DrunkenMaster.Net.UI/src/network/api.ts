import { IMockRoute } from "../components/MockRoutes";

export const restartAsync = () => {
    return fetch("/drunken-master/api/restart", {
        method: 'POST',
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
}
export const fetchRoutesAsync = () => {
    return fetch("/drunken-master/api/mock-routes", { mode: 'cors' });
}
export const fetchServerConfigAsync = () => {
    return fetch("/drunken-master/api/config", { mode: 'cors' });
}
export const createNewRoute = (newRoute: IMockRoute) => {
    return fetch("/drunken-master/api/mock-routes", {
        method: "POST",
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newRoute)
    });
}
export const updateRoute = (newRoute: IMockRoute) => {
    return fetch("/drunken-master/api/mock-routes", {
        method: "PUT",
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newRoute)
    });
}