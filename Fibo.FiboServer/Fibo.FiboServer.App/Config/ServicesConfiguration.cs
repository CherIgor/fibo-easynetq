﻿namespace Fibo.FiboServer.App.Config
{
    public class ServicesConfiguration
    {
        public string MessageBusConnectionString { get; set; }

        public bool SimulateDelay { get; set; }

        public int DelayMinMs { get; set; }

        public int DelayMaxMs { get; set; }
    }
}
