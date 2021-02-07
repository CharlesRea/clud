import React, { useEffect } from 'react';
import styled from 'styled-components/macro';

import { ListApplicationsQuery } from '../../grpc/clud_pb';
import { ApplicationsClient } from '../../grpc/CludServiceClientPb';
import {
  bold,
  font4xl,
  fontSmall,
  fontXl,
  monospacedFontFamily,
  semibold,
} from '../../styles/fonts';
import { spacing4, spacing6, spacing8, spacing12 } from '../../styles/spacing';
import { matchGrpc, useGrpc } from '../../utils/useGrpc';
import { ApplicationCard } from './ApplicationCard';
import { Applications } from './Applications';
import { ReactComponent as Logo } from './logo.svg';

export const Home = () => (
  <Container>
    <Header />
    <Applications />
  </Container>
);

const Container = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  max-width: 1800px;
  width: 100%;
  margin: auto;
`;

const Header = () => (
  <HeaderContainer>
    <a href="/">
      <StyledLogo />
      <CludName>clud</CludName>
    </a>
    <SubTitle>The Ghyston cloud</SubTitle>
    <Description>
      Easy deployment for your apps and side projects by running{' '}
      <code className="font-mono font-bold tracking-tight text-gray-100">clud deploy</code>.
      Out-the-box support for common frameworks & databases, or custom deployments using a
      Dockerfile.
    </Description>
    <Links>
      <Link
        href="https://github.com/CharlesRea/clud"
        className="flex items-center link link-secondary"
      >
        Docs
      </Link>
      <Link
        href="https://github.com/CharlesRea/clud"
        className="flex items-center link link-secondary ml-8"
      >
        Github
      </Link>
    </Links>
  </HeaderContainer>
);

const HeaderContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: ${spacing4};
  margin-bottom: ${spacing12};
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
`;

const SubTitle = styled.h2`
  margin-bottom: ${spacing8};
  font-size: ${fontSmall};
  font-weight: ${semibold};
`;

const Description = styled.p`
  max-width: 30em;
  text-align: center;
  margin-bottom: ${spacing6};
  font-size: ${fontXl};
`;

const Links = styled.div`
  display: flex;
  flex-direction: row;
`;

const Link = styled.a`
  &:not(:last-child) {
    margin-right: ${spacing8};
  }
  font-size: ${fontXl};
`;
