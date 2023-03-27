namespace Fibo.FiboConsole.Config
{
    public static class Configuration
    {
        public const int DefaultCalculationsCount = 3;

        public const int CalculationRetryTimeInMs = 4000;

        public const string MessageBusConnectionString = "host=localhost";

        public const string ApiServiceHost = "http://localhost:5020";
    }
}
