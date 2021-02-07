import React, { useEffect } from 'react';

import { ListApplicationsQuery } from '../grpc/clud_pb';
import { ApplicationsClient } from '../grpc/CludServiceClientPb';
import { matchGrpc, useGrpc } from '../utils/useGrpc';

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
              <div key={app.getName()}>{app.getName()}</div>
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
