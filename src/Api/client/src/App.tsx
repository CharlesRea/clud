import grpcWeb from 'grpc-web';
import React, { useCallback, useEffect, useState } from 'react';

import { ListApplicationsQuery } from './grpc/clud_pb';
import { ApplicationsClient } from './grpc/CludServiceClientPb';

// TODO parameterise this
const applicationsClient = new ApplicationsClient(`https://localhost:5000`);

type GrpcRequestState<TResponse> = {
  isLoading: boolean;
  error: grpcWeb.Error | null;
  response: TResponse | null;
};

type Result<T> = { succeeded: true; result: T } | { succeeded: false; error: grpcWeb.Error };

const useGrpcRequest = <TClient, TRequest, TResponse>(
  client: TClient,
  request: (
    params: TRequest,
    metadata: grpcWeb.Metadata | null,
    callback: (err: grpcWeb.Error, response: TResponse) => void,
  ) => void,
): [GrpcRequestState<TResponse>, (params: TRequest) => Promise<Result<TResponse>>] => {
  const [state, setState] = useState<GrpcRequestState<TResponse>>({
    isLoading: false,
    error: null,
    response: null,
  });

  const sendRequest = useCallback(
    async (params: TRequest): Promise<Result<TResponse>> => {
      setState((state) => ({ ...state, isLoading: true }));

      return new Promise((resolve, reject) => {
        request.bind(client)(params, null, (error, response) => {
          if (response) {
            setState((state) => ({
              ...state,
              isLoading: false,
              error: null,
              response: response,
            }));
            resolve({ succeeded: true, result: response });
          } else if (error) {
            setState((state) => ({
              ...state,
              isLoading: false,
              error,
            }));
            resolve({ succeeded: false, error });
          } else {
            reject(new Error('No response or error received from GRPC callback'));
          }
        });
      });
    },
    [client, request],
  );

  return [state, sendRequest];
};

export const App = () => {
  const [applications, fetchApplications] = useGrpcRequest(
    applicationsClient,
    applicationsClient.listApplications,
  );

  console.log('Rendering');
  useEffect(() => {
    fetchApplications(new ListApplicationsQuery());
  }, [fetchApplications]);

  return (
    <div className="p-8">
      {applications.isLoading && <div>Loading</div>}
      {applications.error && (
        <div>
          error: {applications.error.message}. Error code: {applications.error.code}
        </div>
      )}
      {applications.response && (
        <div>
          {applications.response.getApplicationsList().map((app) => (
            <div key={app.getName()}>{app.getName()}</div>
          ))}
        </div>
      )}
    </div>
  );
};
