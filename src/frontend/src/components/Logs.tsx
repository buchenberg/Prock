import { ReactNode, useState } from "react";
import { Card, Container } from "react-bootstrap";
import signalR from "use-signalr-hub"

const ProckNotification = ({ date, entry }: { date: string, entry: string }) => (
    <span><span className="text-muted">{date}:</span> {entry}</span>
)

export default function Logs() {
    const [notifications, setNotifications] = useState<ReactNode[]>([]);

    //This is a hook and not a side-effect
    signalR.useHub("http://localhost:5001/prock/api/signalr", {
        onConnected: (hub) => {
            hub.on("ProxyRequest", (x: string) => {
                setNotifications((p) => [...p,
                <ProckNotification date={new Date().toISOString()} entry={x} />
                ])
            });
        },
        onDisconnected: () => {
            //nada
        },
        onError: (error) => {
            console.error("signalr connection error:", error);
        }
    });


    return (
        <Container fluid className='mt-3'>
            <div className='mb-3'>
                <h4>Logs</h4>
            </div>
            <Card body bg='black' className="overflow-auto" style={{ height: '75vh' }}>
                <div className="lh-1 font-monospace">
                    <div className="text-muted mb-2">ProckShell</div>
                    {notifications.map((x, i) =>
                        <div key={i}>{x}</div>
                    )}
                </div>
            </Card>
        </Container>
    )
}