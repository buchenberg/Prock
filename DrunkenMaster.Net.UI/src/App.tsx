import { useState } from 'react';
import './App.css'
import MockRoutes from './components/MockRoutes'
import { Container, Modal, Navbar, Spinner, Tab, Tabs } from 'react-bootstrap'
import { ArrowCounterclockwise } from 'react-bootstrap-icons';
import * as api from './network/api';
import Config from './components/Config';

function App() {
  const delay = (ms: number) => new Promise(res => setTimeout(res, ms));
  const [showRestartModal, setShowResartModal] = useState(false);

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
      const response = await api.restartAsync();
      if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
      }
    } catch (error) {
      if (error instanceof Error) {
        console.error(error.message);
      } else {
        console.error(error);
      }
    }
  }

  return (
    <>
      <Navbar data-bs-theme="dark" className="bg-body-tertiary" expand="lg">
        <Container>
          <Navbar.Brand href="#home">Drunken Master</Navbar.Brand>
          <ArrowCounterclockwise className="float-end icon-btn" onClick={handleRestart} />
        </Container>
      </Navbar>

      <Container className="mt-3">
        <Tabs
          defaultActiveKey="home"
          id="uncontrolled-content-tabs"
          className="mb-3">
          <Tab eventKey="home" title="Home">
            <Config />
          </Tab>
          <Tab eventKey="mocks" title="Mocks">
            <MockRoutes />
          </Tab>
        </Tabs>
      </Container>

      <Modal show={showRestartModal}
        onHide={handleCloseRestartModal}
        backdrop="static"
        keyboard={false}
        centered>
        <Modal.Body>
          <p><b>Drunken Master is restarting...</b></p>
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

export default App
