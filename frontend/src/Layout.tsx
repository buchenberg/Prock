import { Navbar, Container, Nav, NavDropdown } from "react-bootstrap";
import { Outlet } from "react-bootstrap-icons";

export const Layout = ({ onRestart }: {onRestart: () => void}) => (
    <>
        <Navbar expand="lg">
            <Container fluid>
                <Navbar.Brand href="#"><span className='text-danger'>Prock</span> <small className='text-muted'>Mocking Proxy</small></Navbar.Brand>
                <Navbar.Toggle aria-controls="navbar" />
                <Navbar.Collapse id="navbar">
                    <Nav className='ms-auto'>
                        <NavDropdown title="Admin" id="nav-dropdown" drop={"start"}>
                            <NavDropdown.Item eventKey="swagger" as="a" target="_blank" rel="noopener noreferrer" href='/swagger/index.html'>Open Swagger UI</NavDropdown.Item>
                            <NavDropdown.Item eventKey="restart" as="button" onClick={onRestart}>Restart Service</NavDropdown.Item>
                        </NavDropdown>
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
        <Container fluid>
            <Outlet />
        </Container>
    </>
)