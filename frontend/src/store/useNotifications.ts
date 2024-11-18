import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useState, useEffect } from "react";

export const useNotifications = () => {
    const [connectionRef, setConnection] = useState<HubConnection>();
    const [notifications, setNotifications] = useState<string[]>([]);


    function createHubConnection() {
        const backendHost = import.meta.env.VITE_BACKEND_HOST ?? "http://localhost";
        const backendPort = import.meta.env.VITE_BACKEND_PORT ?? "5001";
        const con = new HubConnectionBuilder()
            .withUrl(`${backendHost}:${backendPort}/prock/api/signalr`)
            .withAutomaticReconnect()
            .build();
        setConnection(con);
    }

    useEffect(() => {
        createHubConnection();
    });

    useEffect(() => {
        if (connectionRef) {
            connectionRef
                .start()
                .then(() => {
                    connectionRef.on("ProxyRequest", data => {
                        setNotifications((prev) => [...prev, data]);
                    });
                })
                .catch((err) => {
                    console.error(`Error: ${err}`);
                });

        }
    }, [connectionRef]);

    return notifications;
};