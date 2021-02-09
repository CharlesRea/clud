import { MemoryRouter } from 'react-router-dom';
import { GlobalStyles } from '../src/styles/GlobalStyles';
import { addDecorator } from '@storybook/react';
import { Background } from '../src/Background';

export const parameters = {
  actions: { argTypesRegex: '^on[A-Z].*' },
  options: { showPanel: false },
};

const decorator = (storyFn) => (
  <MemoryRouter>
    <GlobalStyles />
    <Background />
    {storyFn()}
  </MemoryRouter>
);

addDecorator(decorator);
