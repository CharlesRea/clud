import React, { useEffect } from 'react';
import styled from 'styled-components/macro';

import { ListApplicationsQuery } from '../../grpc/clud_pb';
import { ApplicationsClient } from '../../grpc/CludServiceClientPb';
import { spacing8 } from '../../styles/spacing';
import { matchGrpc, useGrpc } from '../../utils/useGrpc';
import { ApplicationCard } from './ApplicationCard';

// TODO parameterise this
const applicationsClient = new ApplicationsClient(`https://localhost:5001`);

export const Applications = () => {
  const [applications, listApplications] = useGrpc(
    applicationsClient,
    applicationsClient.listApplications,
  );
  useEffect(() => {
    listApplications(new ListApplicationsQuery());
  }, [listApplications]);

  return (
    <div>
      {matchGrpc(applications, {
        Empty: () => <div>Empty</div>,
        Loading: () => <div>Loading</div>,
        Success: (applications) => (
          <div>
            {applications.response.getApplicationsList().map((app) => (
              <StyledApplicationCard key={app.getName()} application={app} />
            ))}
          </div>
        ),
        Error: (applications) => (
          <div>
            error: {applications.error.message}. Error code: {applications.error.code}
          </div>
        ),
      })}
    </div>
  );
};

const StyledApplicationCard = styled(ApplicationCard)`
  margin-bottom: ${spacing8};
`;
