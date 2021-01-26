import React, { useEffect, useState } from 'react';

import { ListApplicationsQuery, ListApplicationsResponse } from './grpc/clud_pb';
import { ApplicationsClient } from './grpc/CludServiceClientPb';

// TODO parameterise this
const applicationsClient = new ApplicationsClient(`https://localhost:5001`);

export const App = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [response, setResponse] = useState<ListApplicationsResponse | null>(null);

  useEffect(() => {
    setLoading(true);
    const request = new ListApplicationsQuery();
    applicationsClient.listApplications(request, {}, (error, response) => {
      if (error) {
        setError(error);
      } else {
        setResponse(response);
      }
    });
  }, []);

  return (
    <div>
      {loading && <div>Loading</div>}
      {error && <div>error: {error}</div>}
      {response && (
        <div>
          {response.getApplicationsList().map((app) => (
            <div key={app.getName()}>{app.getName()}</div>
          ))}
        </div>
      )}
    </div>
  );
};
