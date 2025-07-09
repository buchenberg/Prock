import { useState } from 'react';
import './App.css'
import MockRoutes from './components/MockRoutes'
import { Container, Modal, Nav, Navbar, NavDropdown, Spinner, Tab, Tabs } from 'react-bootstrap'
import * as api from './network/api';
import Config from './components/Config';
import axios from 'axios';
import Logs from './components/Logs';
import OpenApiDocuments from './components/OpenApiDocuments/OpenApiDocuments';
import { useLocation, useNavigate } from 'react-router-dom';


import '@scalar/api-reference-react/style.css'
import Home from './components/Home';

function App() {
  const delay = (ms: number) => new Promise(res => setTimeout(res, ms));
  const [showRestartModal, setShowResartModal] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const [key, setKey] = useState(location.hash ? location.hash : "#home");

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
              <NavDropdown title="Admin" id="nav-dropdown" drop={"start"}>
                <NavDropdown.Item eventKey="swagger" as="a" target="_blank" rel="noopener noreferrer" href='/swagger/index.html'>Open Swagger UI</NavDropdown.Item>
                <NavDropdown.Item eventKey="restart" as="button" onClick={handleRestart}>Restart Service</NavDropdown.Item>
              </NavDropdown>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container fluid>
        <Tabs
          id="page-tabs"
          activeKey={key}
          onSelect={(k) => {
            navigate(k as string);
            setKey(k as string)
          }}>
          <Tab eventKey="#home" title="Home">
            <Home />
          </Tab>
          <Tab eventKey="#config" title="Configuration">
            <Config />
          </Tab>
          <Tab eventKey="#openapi" title="OpenAPI">
            <OpenApiDocuments />
          </Tab>
          <Tab eventKey="#mocks" title="Mocks">
            <MockRoutes />
          </Tab>

          <Tab eventKey="#logs" title="Logs">
            <Logs />
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