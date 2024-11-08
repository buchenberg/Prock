export const restartAsync = () => {
    return fetch("http://localhost:5001/drunken-master/api/restart", {
        method: 'POST',
        mode: 'cors',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });
}

export const fetchRoutesAsync = () => {
    return fetch("http://localhost:5001/drunken-master/api/mock-routes", { mode: 'cors' });
}
export const fetchServerConfigAsync = () => {
    return fetch("http://localhost:5001/drunken-master/api/config", { mode: 'cors' });
}