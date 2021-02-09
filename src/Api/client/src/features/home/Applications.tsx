import React, { useEffect } from 'react';
import styled from 'styled-components/macro';

import { ListApplicationsQuery, ListApplicationsResponse } from '../../grpc/clud_pb';
import { GrpcResponse } from '../../shared/GrpcResponse';
import { spacing12 } from '../../styles/spacing';
import { applicationsClient } from '../../utils/grpc';
import { useGrpc } from '../../utils/useGrpc';
import { ApplicationCard } from './ApplicationCard';

export const Applications = () => {
  const [applications, listApplications] = useGrpc(
    applicationsClient,
    applicationsClient.listApplications,
  );
  useEffect(() => {
    listApplications(new ListApplicationsQuery());
  }, [listApplications]);

  return (
    <GrpcResponse request={applications}>
      {(applications) => <ApplicationsList applications={applications} />}
    </GrpcResponse>
  );
};

export const ApplicationsList = ({ applications }: { applications: ListApplicationsResponse }) => (
  <div>
    {applications.getApplicationsList().map((app) => (
      <StyledApplicationCard key={app.getName()} application={app} />
    ))}
  </div>
);

const StyledApplicationCard = styled(ApplicationCard)`
  &:not(:last-child) {
    margin-bottom: ${spacing12};
  }
`;
