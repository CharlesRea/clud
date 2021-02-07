import React from 'react';
import styled from 'styled-components/macro';

import { Background } from './Background';
import { Home } from './features/home/Home';
import { GlobalStyles } from './styles/GlobalStyles';

export const App = () => (
  <>
    <GlobalStyles />
    <Background />
    <AppContainer>
      <Home />
    </AppContainer>
  </>
);

const AppContainer = styled.div`
  position: absolute;
  min-width: 100vw;
  min-height: 100vh;
`;
