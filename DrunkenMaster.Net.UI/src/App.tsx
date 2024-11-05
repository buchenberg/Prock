import './App.css'
import MockRoutes from './MockRoutes'
import { Container, Navbar } from 'react-bootstrap'

function App() {

  return (
    <>
      <Navbar data-bs-theme="dark" className="bg-body-tertiary" expand="lg">
        <Container>
          <Navbar.Brand href="#home">Drunken Master</Navbar.Brand>
        </Container>
      </Navbar>
      <MockRoutes />
    </>
  )
}

export default App
