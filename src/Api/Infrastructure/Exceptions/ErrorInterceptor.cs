using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Clud.Api.Infrastructure.Exceptions
{
    public class ErrorInterceptor : Interceptor
    {
        private readonly ILogger<ErrorInterceptor> logger;

        public ErrorInterceptor(ILogger<ErrorInterceptor> logger)
        {
            this.logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"An error occured when calling {context.Method}");

                switch (e)
                {
                    case RpcException _:
                        throw;
                    default:
                        throw new RpcException(Status.DefaultCancelled, e.Message);
                }
            }
        }
    }
}
