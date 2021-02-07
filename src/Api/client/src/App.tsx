import grpcWeb from 'grpc-web';
import React, { useCallback, useEffect, useState } from 'react';

import { ListApplicationsQuery } from './grpc/clud_pb';
import { ApplicationsClient } from './grpc/CludServiceClientPb';
import { matchGrpc, useGrpc } from './utils/useGrpc';

// TODO parameterise this
const applicationsClient = new ApplicationsClient(`https://localhost:5001`);

export const App = () => {
  const [applications, listApplications] = useGrpc(
    applicationsClient,
    applicationsClient.listApplications,
  );
  useEffect(() => {
    listApplications(new ListApplicationsQuery());
  }, [listApplications]);

  return (
    <div className="p-8">
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
