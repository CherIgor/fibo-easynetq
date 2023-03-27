using EasyNetQ;
using Fibo.FiboConsole.Calculation;
using Fibo.FiboConsole.Config;
using Microsoft.Extensions.Logging;

namespace Fibo.FiboConsole
{
    internal class Program
    {
        private const string COMMAND_STATUS = "status";
        private const string COMMAND_STOP = "stop";
        private const string COMMAND_EXIT = "exit";

        static void Main(string[] args)
        {
            int calculationsCount;
            var isCalculationsCountFromArgs = true;
            if (!(args.Length > 0
                && Int32.TryParse(args[0], out calculationsCount)
                && calculationsCount > 0))
            {
                calculationsCount = Configuration.DefaultCalculationsCount;
                isCalculationsCountFromArgs = false;
            }

            WriteToConsole(() =>
            {
                Console.WriteLine($"Will run {calculationsCount} parallel calculations (got from {(isCalculationsCountFromArgs ? "program arguments" : "configuration")}).");
            });

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Fibo", LogLevel.Information)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<FiboCalculation>();

            var calculations = new List<FiboCalculation>();
            using var cancelTokenSource = new CancellationTokenSource();
            using var bus = RabbitHutch.CreateBus(Configuration.MessageBusConnectionString);

            for (var index = 0; index < calculationsCount; index++)
            {
                var guid = Guid.NewGuid();
                var calculation = new FiboCalculation(guid, bus, logger, Configuration.CalculationRetryTimeInMs);
                calculations.Add(calculation);
                Task.Run(() => calculation.CalculateFiboncacciSequenceAsync(cancelTokenSource.Token));
            }

            WriteToConsole(() =>
            {
                Console.WriteLine($"{calculationsCount} tasks started");
            });

            Thread.Sleep(100);
            while (true)
            {
                WriteToConsole(() =>
                {
                    Console.WriteLine($"Input \"{COMMAND_STATUS}\" to display current progress of calculation");
                    Console.WriteLine($"Input \"{COMMAND_STOP}\" to stop the calculations");
                    Console.WriteLine($"Input \"{COMMAND_EXIT}\" to exit");
                });

                var userInput = Console.ReadLine()?.Trim()?.ToLower();
                WriteToConsole(() =>
                {
                    Console.WriteLine("==================================================");
                });

                if (userInput == COMMAND_STATUS)
                {
                    WriteToConsole(() =>
                    {
                        Console.WriteLine($"Fibo app. Calculating {calculationsCount} Fibonacci sequences.");
                        foreach (var calcProcessor in calculations)
                        {
                            Console.WriteLine($"Sequence {calcProcessor.GetProgress()}");
                        }
                    });
                }
                else if (userInput == COMMAND_STOP)
                {
                    cancelTokenSource.Cancel();
                    foreach (var calcProcessor in calculations)
                    {
                        calcProcessor.Dispose();
                    }

                    WriteToConsole(() =>
                    {
                        Console.WriteLine($"Calculations have been stopped");
                    });
                }
                else if (userInput == COMMAND_EXIT)
                {
                    cancelTokenSource.Cancel();
                    foreach (var calcProcessor in calculations)
                    {
                        calcProcessor.Dispose();
                    }

                    WriteToConsole(() =>
                    {
                        Console.WriteLine($"Calculations have been stopped");
                    });

                    break;
                }
            }

            WriteToConsole(() =>
            {
                Console.WriteLine($"Fibo app Exited");
            });
        }

        private static void WriteToConsole(Action action)
        {
            var consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                action();
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
    }
}
