using Grpc.Core;

namespace Clud.Api.Infrastructure.Exceptions
{
    public class NotFoundException : RpcException
    {
        public NotFoundException(string message, string userVisibleMessage = null) :
            base(new Status(StatusCode.NotFound, userVisibleMessage), message)
        { }
    }
}