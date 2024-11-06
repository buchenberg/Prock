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