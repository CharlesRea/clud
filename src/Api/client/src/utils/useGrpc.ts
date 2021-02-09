import grpcWeb from 'grpc-web';
import { useCallback, useState } from 'react';

import { match, Pattern } from './PatternMatching';

export type GrpcState<TResponse> =
  | { status: 'Empty'; response: null }
  | { status: 'Loading'; response: TResponse | null }
  | { status: 'Success'; response: TResponse }
  | { status: 'Error'; response: TResponse | null; error: grpcWeb.Error };

type Result<T> = { succeeded: true; result: T } | { succeeded: false; error: grpcWeb.Error };

export const useGrpc = <TClient, TRequest, TResponse>(
  client: TClient,
  request: (
    params: TRequest,
    metadata: grpcWeb.Metadata | null,
    callback: (err: grpcWeb.Error, response: TResponse) => void,
  ) => void,
): [GrpcState<TResponse>, (params: TRequest) => Promise<Result<TResponse>>] => {
  const [state, setState] = useState<GrpcState<TResponse>>({
    status: 'Empty',
    response: null,
  });

  const sendRequest = useCallback(
    async (params: TRequest): Promise<Result<TResponse>> => {
      setState((state) => ({ status: 'Loading', response: state.response }));

      return new Promise((resolve, reject) => {
        request.bind(client)(params, null, (error, response) => {
          if (response) {
            setState((state) => ({
              status: 'Success',
              response: response,
            }));
            resolve({ succeeded: true, result: response });
          } else if (error) {
            setState((state) => ({
              status: 'Error',
              error,
              response: state.response,
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

export const matchGrpc = <TResponse, TOutput>(
  input: GrpcState<TResponse>,
  pattern: Pattern<GrpcState<TResponse>, 'status', TOutput>,
) => match(input, 'status', pattern);
