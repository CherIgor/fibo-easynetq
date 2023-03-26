using System.Net;

namespace Fibo.FiboConsole.Services
{
    public interface IMsgSender<T> : IDisposable
    {
        Task<(bool ok, HttpStatusCode statusCode, string? message)> SendMsg(T msg, CancellationToken cancellationToken = default);
    }
}
