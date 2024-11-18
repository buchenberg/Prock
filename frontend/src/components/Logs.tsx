import { ReactNode, useState } from "react";
import { Card, Container } from "react-bootstrap";
import signalR from "use-signalr-hub"

export default function Logs() {
    const [notifications, setNotifications] = useState<ReactNode[]>([]);
    signalR.useHub("http://localhost:5001/prock/api/signalr", {
        onConnected: (hub) => {
            hub.on("ProxyRequest", (x) => {
                const date = new Date();
                setNotifications((p) => [...p, <><span className="text-muted">{date.toISOString()}:</span> {x}</>])
            });
        },
        onDisconnected: (error) => {
            console.log("onDisconnected", error);
        },
        onError: (error) => {
            console.log("onError", error);
        }
    });


    return (
        <Container fluid className='mt-3'>
            <div className='mb-3'>
                <h4>Logs</h4>
            </div>
            <Card body bg='black' className="overflow-auto" style={{ height: '75vh' }}>
                <p className="lh-1 font-monospace">
                    <div className="text-muted mb-2">ProckShell</div>
                    {notifications.map((x, i) =>
                        <div key={i}>{x}</div>
                    )}
                </p>
            </Card>
        </Container>
    )
}