import { ReactNode, useEffect, useState } from 'react';
import './App.css'
import MockRoutes from './components/MockRoutes'
import { Button, Container, Modal, Nav, Navbar, NavDropdown, Spinner, Stack, Tab, Tabs } from 'react-bootstrap'
import { ArrowCounterclockwise, Braces, DoorOpen } from 'react-bootstrap-icons';
import * as api from './network/api';
import Config from './components/Config';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Terminal, { ColorMode, TerminalOutput } from 'react-terminal-ui';
import axios from 'axios';

function App() {
  const delay = (ms: number) => new Promise(res => setTimeout(res, ms));
  const [showRestartModal, setShowResartModal] = useState(false);
  const [terminalLineData, setTerminalLineData] = useState<ReactNode[]>([]);

  const backendHost = import.meta.env.VITE_BACKEND_HOST ?? "http://localhost";
  const backendPort = import.meta.env.VITE_BACKEND_PORT ?? "5001";
  console.log(`SignalR endpoint: ${backendHost}:${backendPort}/prock/signalr`);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${backendHost}:${backendPort}/prock/signalr`)
      .build();
    connection.start();
    connection.on("ProxyRequest", data => {
      setTerminalLineData((prev) => [...prev, <TerminalOutput>{data}</TerminalOutput>])
    });
  }, [backendHost, backendPort, terminalLineData])

  const handleCloseRestartModal = () => {
    setShowResartModal(false);
  }
  const handleShowResartModal = () => {
    setShowResartModal(true);
    delay(7000)
      .then(() => {
        handleCloseRestartModal();
      });
  }

  const handleRestart = async () => {
    handleShowResartModal();
    try {
      await api.restartAsync();
    }
    catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        console.error(error);
      } else {
        console.error(error);
      }
    }
  }

  return (
    <>
      <Navbar expand="lg">
        <Container fluid>
          <Navbar.Brand href="#"><span className='text-danger'>Prock</span> <small className='text-muted'>Mocking Proxy</small></Navbar.Brand>
          <Navbar.Toggle aria-controls="navbar" />
          <Navbar.Collapse id="navbar">
            <Nav className='ms-auto'>
              <NavDropdown title="Prock Server Menu" id="nav-dropdown">
                <NavDropdown.Item eventKey="swagger" as="a" target="_blank" rel="noopener noreferrer" href='/swagger/index.html'>Open Swagger UI</NavDropdown.Item>
                <NavDropdown.Item eventKey="restart" as="button" onClick={handleRestart}>Restart Service</NavDropdown.Item>
              </NavDropdown>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>

      <Container fluid>
        <Tabs
          defaultActiveKey="home"
          id="content-tabs"
          className="mb-3">
          <Tab eventKey="home" title="Home">
            <Container className='mt-3' fluid>
              <Config />
            </Container>
          </Tab>
          <Tab eventKey="mocks" title="Mocks">
            <Container className='mt-3' fluid>
              <MockRoutes />
            </Container>
          </Tab>
          <Tab eventKey="signals" title="Signals">
            <Container className='mt-3' fluid>
                <Terminal name='Proxy Requests' colorMode={ColorMode.Dark} onInput={terminalInput => console.log(`New terminal input received: '${terminalInput}'`)}>
                  {terminalLineData}
                </Terminal>
            </Container>
          </Tab>
        </Tabs>
      </Container>

      <Modal show={showRestartModal}
        onHide={handleCloseRestartModal}
        backdrop="static"
        keyboard={false}
        centered>
        <Modal.Body>
          <p><b>Prock is restarting...</b></p>
          <blockquote>
            <p className="mb-0">"Do not fight with the strength, absorb it, and it flows, use it."</p>
            <footer className='float-end'>- <cite title="Yip Man">Yip Man</cite></footer>
          </blockquote>
          <div className='p-4 mt-3'>
            <div className="position-absolute top-80 start-50 translate-middle">
              <Spinner animation="border" role="status" variant="warning">
                <span className="visually-hidden">Restarting...</span>
              </Spinner>
            </div>
          </div>
        </Modal.Body>
      </Modal>
    </>
  )
}

export default App;