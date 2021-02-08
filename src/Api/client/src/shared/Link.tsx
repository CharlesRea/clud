import { Link as RouterLink } from 'react-router-dom';
import styled from 'styled-components/macro';

import { orange100, orange200, orange300 } from '../styles/colours';
import { letterSpacingWide, semibold } from '../styles/fonts';

export const Link = styled(RouterLink)`
  text-transform: uppercase;
  letter-spacing: ${letterSpacingWide};
  font-weight: ${semibold};
  color: ${orange300};

  &:hover {
    color: ${orange100};
  }

  &:hover {
    color: ${orange200};
  }
`;
