using Fibo.FiboConsole.Config;
using Fibo.FiboConsole.Helpers;
using System.Net;

namespace Fibo.FiboConsole.Services
{
    public class FiboRestMsgSender<T>: IMsgSender<T>, IDisposable
    {
        private readonly HttpClient _client;
        private string _apiServiceHost;
        private readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromSeconds(20);

        public FiboRestMsgSender()
        {
            _client = new HttpClient();
            _client.Timeout = REQUEST_TIMEOUT;
            _apiServiceHost = Configuration.ApiServiceHost;
        }

        public async Task<(bool ok, HttpStatusCode statusCode, string? message)> SendMsg(T msg, CancellationToken cancellationToken = default)
        {
            var url = $"{_apiServiceHost}/api/fibo/next";
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = HttpRequestSerializer.ToJson(msg);
                var response = await _client.SendAsync(request, cancellationToken);
                var message = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return (ok: false, statusCode: response.StatusCode, message);
                }

                return (ok: true, statusCode: response.StatusCode, message);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
