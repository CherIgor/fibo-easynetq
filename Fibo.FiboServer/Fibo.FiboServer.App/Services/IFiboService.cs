using Fibo.Shared;

namespace Fibo.FiboServer.App.Services
{
    public interface IFiboService
    {
        FiboMessage CalculateNextMessage(FiboMessage msg);
        void SendMessage(FiboMessage msg);
    }
}
