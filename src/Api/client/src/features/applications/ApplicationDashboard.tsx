import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import styled from 'styled-components/macro';

import { ApplicationQuery, ApplicationResponse } from '../../grpc/clud_pb';
import { GrpcResponse } from '../../shared/GrpcResponse';
import { PageContent } from '../../shared/PageContent';
import { shadowSm } from '../../styles/shadows';
import { spacing8, spacing12 } from '../../styles/spacing';
import { assertNotNull } from '../../utils/assertNotNull';
import { applicationsClient } from '../../utils/grpc';
import { useGrpc } from '../../utils/useGrpc';
import { ApplicationHeader } from './ApplicationHeader';

export const ApplicationDashboard = () => {
  const { applicationName } = useParams();
  assertNotNull(applicationName);

  const [application, getApplication] = useGrpc(
    applicationsClient,
    applicationsClient.getApplication,
  );

  useEffect(() => {
    const query = new ApplicationQuery();
    query.setName(applicationName);
    getApplication(query);
  }, [applicationName, getApplication]);

  return (
    <GrpcResponse request={application}>
      {(application) => <Application application={application.toObject()} />}
    </GrpcResponse>
  );
};

// TODO add a "Back to Clud" link floating above the panel
const Application = ({ application }: { application: ApplicationResponse.AsObject }) => (
  <PageContent>
    <Container>
      <ApplicationHeader application={application} />
    </Container>
  </PageContent>
);

const Container = styled.div`
  margin-top: ${spacing8};
  background-color: rgba(0, 20, 40, 0.1);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: ${spacing12};
  box-shadow: ${shadowSm};
  width: 800px;
  max-width: 100%;
`;
