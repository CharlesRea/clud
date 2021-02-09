import React, { useEffect } from 'react';
import styled from 'styled-components/macro';

import { ListApplicationsQuery } from '../../grpc/clud_pb';
import { ApplicationsClient } from '../../grpc/CludServiceClientPb';
import { PageContent } from '../../shared/PageContent';
import { borderRadius2 } from '../../styles/borders';
import { orange100, orange300, teal100, teal300 } from '../../styles/colours';
import {
  bold,
  font4xl,
  fontSmall,
  fontXl,
  letterSpacingWide,
  monospacedFontFamily,
  semibold,
} from '../../styles/fonts';
import { shadowLg, shadowSm } from '../../styles/shadows';
import { spacing1, spacing2, spacing6, spacing8, spacing12, spacing20 } from '../../styles/spacing';
import { Applications } from './Applications';
import { ReactComponent as Logo } from './logo.svg';

export const Home = () => (
  <Container>
    <InnerContainer>
      <HomeHeader />
      <Applications />
    </InnerContainer>
  </Container>
);

const Container = styled(PageContent)``;

const InnerContainer = styled.div`
  margin-top: ${spacing8};
  background-color: rgba(0, 20, 40, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: ${spacing12};
  box-shadow: ${shadowSm};
  width: 800px;
  max-width: 100%;
`;

export const HomeHeader = () => (
  <HeaderContainer>
    <LogoLink href="/">
      <StyledLogo />
      <CludName>clud</CludName>
    </LogoLink>
    <SubTitle>The Ghyston cloud</SubTitle>
    <Description>
      Easy deployment for your apps and side projects: just run <Code>clud deploy</Code>.
      Out-of-the-box support for common frameworks & databases, or custom deployments using a
      Dockerfile.
    </Description>
    <Links>
      <HeaderLink
        href="https://github.com/CharlesRea/clud"
        className="flex items-center link link-secondary"
      >
        Docs
      </HeaderLink>
      <HeaderLink
        href="https://github.com/CharlesRea/clud"
        className="flex items-center link link-secondary ml-8"
      >
        Github
      </HeaderLink>
    </Links>
  </HeaderContainer>
);

const HeaderContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: ${spacing12};
`;

const LogoLink = styled.a`
  color: ${teal300};
  &:hover {
    color: ${teal100};
  }
`;

const StyledLogo = styled(Logo)`
  width: 120px;
  height: 120px;
`;

const CludName = styled.h1`
  font-weight: ${semibold};
  font-size: ${font4xl};
  text-align: center;
  font-family: ${monospacedFontFamily};
  line-height: 1;
  margin-top: -${spacing2};
  margin-bottom: ${spacing1};
`;

const SubTitle = styled.h2`
  margin-bottom: ${spacing8};
  font-size: ${fontSmall};
  font-weight: ${semibold};
`;

const Description = styled.p`
  max-width: 33em;
  text-align: center;
  margin-bottom: ${spacing6};
  font-size: ${fontXl};
`;

const Code = styled.code`
  font-family: ${monospacedFontFamily};
  font-weight: ${bold};
  color: ${orange300};
`;

const Links = styled.div`
  display: flex;
  flex-direction: row;
`;

const HeaderLink = styled.a`
  text-transform: uppercase;
  letter-spacing: ${letterSpacingWide};
  font-weight: ${semibold};
  color: ${orange300};

  &:hover {
    color: ${orange100};
  }

  &:not(:last-child) {
    margin-right: ${spacing20};
  }
`;
