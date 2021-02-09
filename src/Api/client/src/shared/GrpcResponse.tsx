import React from 'react';

import { GrpcState, matchGrpc } from '../utils/useGrpc';

export const GrpcResponse = <TResponse extends unknown>({
  request,
  children,
}: {
  request: GrpcState<TResponse>;
  children: (response: TResponse) => React.ReactNode;
}) => (
  <>
    {matchGrpc(request, {
      Empty: () => <div>Empty</div>,
      Loading: () => <div>Loading</div>,
      Success: (response) => children(response.response),
      Error: (response) => (
        <div>
          GRPC error: {response.error.message}. Error code: {response.error.code}
        </div>
      ),
    })}
  </>
);
