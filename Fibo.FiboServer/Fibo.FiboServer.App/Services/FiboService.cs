using EasyNetQ;
using Fibo.FiboServer.App.Helpers;
using Fibo.Shared;

namespace Fibo.FiboServer.App.Services
{
    public class FiboService : IFiboService
    {
        private readonly IBus _bus;

        public FiboService(
            IBus bus
        )
        {
            _bus = bus;
        }

        /// <exception cref="OverflowException">Arithmetic operation resulted in an overflow</exception>
        public FiboMessage CalculateNextMessage(FiboMessage msg)
        {
            var nextValue = FibonacciCalculator.GetNext(msg.PrevValue, msg.Value);
            return new FiboMessage()
            {
                KeyId = msg.KeyId,
                PrevValue = msg.Value,
                Value = nextValue,
                IterationIndex = msg.IterationIndex + 1,
            };
        }

        public void SendMessage(FiboMessage msg)
        {
            _bus.PubSub.Publish<FiboMessage>(msg, topic: msg.KeyId);
        }
    }
}
