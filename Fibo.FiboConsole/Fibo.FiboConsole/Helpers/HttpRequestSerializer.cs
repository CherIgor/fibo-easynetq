using System.Text;
using System.Text.Json;

namespace Fibo.FiboConsole.Helpers
{
    public static class HttpRequestSerializer
    {
        public const string AppJsonMediaType = "application/json";

        public static StringContent ToJson<T>(T data)
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            string json = JsonSerializer.Serialize(data, options);
            return new StringContent(json, Encoding.UTF8, AppJsonMediaType);
        }
    }
}
