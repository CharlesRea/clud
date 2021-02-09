import styled from 'styled-components/macro';

import { baseHostname } from '../../config';
import { ApplicationResponse } from '../../grpc/clud_pb';
import { Button } from '../../shared/Button';
import { ExternalLink } from '../../shared/Link';
import { neutral200, orange300 } from '../../styles/colours';
import { font2xl } from '../../styles/fonts';
import { spacing2, spacing4, spacing8, spacing12 } from '../../styles/spacing';
import { GitRepoLink } from './GitRepoLink';

export const ApplicationHeader = ({
  application,
}: {
  application: ApplicationResponse.AsObject;
}) => (
  <Container>
    <Header>
      <AppName>{application.name}</AppName>
      {application.ingressurl !== null && <BaseHostname>.{baseHostname}</BaseHostname>}
    </Header>
    {application.description && <Description>{application.description}</Description>}
    {(application.owner || application.repository) && (
      <Metadata>
        {application.owner && <MetadataItem>Created by {application.owner}</MetadataItem>}
        {application.repository && (
          <MetadataItem>
            <GitRepoLink repository={application.repository} />
          </MetadataItem>
        )}
      </Metadata>
    )}
    <Actions>
      {application.ingressurl != null && (
        <Action>
          <ExternalLink href={application.ingressurl.value} target="_blank" rel="noreferrer">
            Link to app
          </ExternalLink>
        </Action>
      )}
      {/* TODO implement config file */}
      <Action>
        <Button onClick={() => alert('TODO')}>Config file</Button>
      </Action>
      {/* TODO implement delete */}
      <Action>
        <Button onClick={() => alert('TODO')}>Delete</Button>
      </Action>
    </Actions>
  </Container>
);

const Container = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
`;

const Header = styled.h1`
  font-size: ${font2xl};
  margin-bottom: ${spacing8};
`;

const AppName = styled.span`
  color: ${orange300};
`;

const BaseHostname = styled.span`
  color: ${neutral200};
`;

const Description = styled.div`
  margin-bottom: ${spacing4};
`;

const Metadata = styled.div`
  margin-bottom: ${spacing4};
  margin-left: -${spacing4};
  margin-right: -${spacing4};
  display: flex;
  flex-direction: row;
  align-items: center;
`;

const MetadataItem = styled.div`
  margin-left: ${spacing4};
  margin-right: ${spacing4};
`;

const Actions = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  margin-top: ${spacing4};
`;

const Action = styled.div`
  margin-left: ${spacing12};
  margin-right: ${spacing12};
`;
