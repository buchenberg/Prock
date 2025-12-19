import { ReactNode, useState } from "react";
import { Card, Container } from "react-bootstrap";
import signalR from "use-signalr-hub"

type LogType = 'proxy' | 'mock';

const ProckNotification = ({ date, entry, type }: { date: string, entry: string, type: LogType }) => {
    const colorClass = type === 'mock' ? 'text-info' : '';
    return (
        <span>
            <span className="text-muted">{date}:</span>{' '}
            <span className={colorClass}>{entry}</span>
        </span>
    );
}

export default function Logs() {
    const [notifications, setNotifications] = useState<ReactNode[]>([]);

    //This is a hook and not a side-effect
    signalR.useHub("http://localhost:5001/prock/api/signalr", {
        onConnected: (hub) => {
            // Proxy requests - default color
            hub.on("ProxyRequest", (x: string) => {
                setNotifications((p) => [...p,
                <ProckNotification date={new Date().toISOString()} entry={x} type="proxy" />
                ])
            });
            // Mock responses - blue
            hub.on("MockResponse", (x: string) => {
                setNotifications((p) => [...p,
                <ProckNotification date={new Date().toISOString()} entry={x} type="mock" />
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
