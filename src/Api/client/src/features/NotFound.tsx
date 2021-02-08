import styled from 'styled-components/macro';

import { Link } from '../shared/Link';
import { neutral200, orange300 } from '../styles/colours';
import { font2xl, font4xl, font6xl, semibold } from '../styles/fonts';
import { spacing2, spacing4, spacing8, spacing12 } from '../styles/spacing';

export const NotFound = () => (
  <Container>
    <Header>404</Header>
    <Description>Whatever you were looking for, it isn't here.</Description>
    <WhatDidYouDo>What did you do?</WhatDidYouDo>
    <Link to="/">Take me home</Link>
  </Container>
);

const Container = styled.div`
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
`;

const Header = styled.h1`
  font-size: 150px;
  font-weight: ${semibold};
`;

const Description = styled.p`
  margin-top: ${spacing8};
  margin-bottom: ${spacing2};
  font-size: ${font2xl};
`;

const WhatDidYouDo = styled.p`
  margin-bottom: ${spacing12};
  color: ${neutral200};
`;

const ReturnToSiteLink = styled(Link)`
  font-size: ${font2xl};
`;
