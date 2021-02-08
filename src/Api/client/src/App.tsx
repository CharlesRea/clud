import React from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import styled from 'styled-components/macro';

import { Background } from './Background';
import { ApplicationDashboard } from './features/applications/ApplicationDashboard';
import { Home } from './features/home/Home';
import { NotFound } from './features/NotFound';
import { GlobalStyles } from './styles/GlobalStyles';

export const App = () => (
  <>
    <GlobalStyles />
    <BrowserRouter>
      <Background />
      <AppContainer>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/apps/:applicationName" element={<ApplicationDashboard />} />
          <Route path="/*" element={<NotFound />} />
        </Routes>
      </AppContainer>
    </BrowserRouter>
  </>
);

const AppContainer = styled.div`
  position: absolute;
  min-width: 100%;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
`;
