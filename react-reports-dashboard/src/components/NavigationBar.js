import React from 'react';
import { Navbar, Nav, Container } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

const NavigationBar = () => {
  return (
    <Navbar bg="dark" variant="dark" expand="lg" sticky="top">
      <Container>
        <Navbar.Brand href="/">
          <i className="bi bi-graph-up me-2"></i>
          Reports Dashboard
        </Navbar.Brand>
        
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <LinkContainer to="/">
              <Nav.Link>Dashboard</Nav.Link>
            </LinkContainer>
            <LinkContainer to="/sales">
              <Nav.Link>Sales Reports</Nav.Link>
            </LinkContainer>
            <LinkContainer to="/performance">
              <Nav.Link>Performance</Nav.Link>
            </LinkContainer>
            <LinkContainer to="/analytics">
              <Nav.Link>Analytics</Nav.Link>
            </LinkContainer>
            <LinkContainer to="/circuit-breaker">
              <Nav.Link>Circuit Breaker</Nav.Link>
            </LinkContainer>
          </Nav>
          
          <Nav>
            <Nav.Link href="http://localhost:16686" target="_blank">
              <i className="bi bi-eye me-1"></i>
              Jaeger Traces
            </Nav.Link>
            <Nav.Link href="http://localhost:3000" target="_blank">
              <i className="bi bi-graph-up-arrow me-1"></i>
              Grafana
            </Nav.Link>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default NavigationBar;