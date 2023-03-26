using EasyNetQ;
using Fibo.FiboConsole.Services;
using Fibo.Shared;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fibo.FiboConsole.Calculation
{
    public class FiboCalculation : IDisposable
    {
        private const int DEFAULT_RETRY_TIME_IN_MS = 4000;

        private readonly Guid _calculationGuid;
        private readonly string _calculationId;
        private readonly IMsgSender<FiboMessage> _msgSender;
        private readonly IBus _bus;
        private readonly int _retryTimeInMs;
        private readonly ILogger? _logger;

        private CancellationToken _cancellationToken;
        private SubscriptionResult? _subscription;

        private List<FiboMessage> _calculatedList = new List<FiboMessage>();
        private object _locker = new object();
        private bool _isStarted = false;
        private bool _isDisposed = false;
        private bool _isFinished = false;
        private bool _hasProgressFromPreviousCheck;

        public FiboCalculation(Guid guid, IBus bus, ILogger? logger, int retryTimeInMs = DEFAULT_RETRY_TIME_IN_MS)
        {
            _calculationGuid = guid;
            _calculationId = guid.ToString();
            _bus = bus;
            _logger = logger;
            _msgSender = new FiboRestMsgSender<FiboMessage>();
            _retryTimeInMs = retryTimeInMs > 0 ? retryTimeInMs : DEFAULT_RETRY_TIME_IN_MS;
        }

        /// <summary>
        /// Start 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task CalculateFiboncacciSequenceAsync(CancellationToken cancellationToken = default)
        {
            lock (_locker)
            {
                if (_isStarted)
                {
                    throw new InvalidOperationException("Calculation has been already started");
                }
                _isStarted = true;
            }

            _cancellationToken = cancellationToken;

            if (IsStopped)
            {
                return;
            }

            _logger?.LogInformation($"Starting {nameof(FiboCalculation)} {_calculationGuid}");

            var initialMsg = GetInitialMessage();
            lock (_locker)
            {
                _calculatedList.Add(initialMsg);
                _hasProgressFromPreviousCheck = false;
            }

            try
            {
                TrySubscribe(throwIfError: true);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{nameof(FiboCalculation)} {_calculationGuid} cannot connect to message bus");
                throw;
            }

            _bus.Advanced.Connected += HandleOnConnected;

            SendMessageAsync(initialMsg);

            while (true)
            {
                await Task.Delay(_retryTimeInMs);

                var progress = GetProgress();

                if (IsStopped)
                {
                    _logger?.LogInformation($"Stopping {nameof(FiboCalculation)}. {progress}");
                    break;
                }

                _logger?.LogInformation($"{nameof(FiboCalculation)} progress: {progress}");
                RetryIfNecessary();
            }
        }

        private void HandleOnConnected(object? sender, ConnectedEventArgs a)
        {
            _logger?.LogDebug($"{nameof(FiboCalculation)} {_calculationGuid} is reconnected");
            if (!IsStopped)
            {
                TrySubscribe();
            }
        }

        private bool TrySubscribe(bool throwIfError = false)
        {
            if (_subscription != null)
            {
                _subscription.Value.Dispose();
                _subscription = null;
            }

            try
            {
                _subscription = _bus.PubSub.Subscribe<FiboMessage>(
                    subscriptionId: _calculationId,
                    OnMsgReceived,
                    config => config
                        .WithTopic(_calculationId)
                        .WithAutoDelete()
                    );
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Cannot subscribe to message bus");
                if (throwIfError)
                {
                    throw;
                }
            }

            return false;
        }

        private void RetryIfNecessary()
        {
            lock (_locker)
            {
                var hasProgress = _hasProgressFromPreviousCheck;
                _hasProgressFromPreviousCheck = false;
                if (!hasProgress)
                {
                    var lastMsg = _calculatedList.LastOrDefault();
                    if (lastMsg != null)
                    {
                        SendMessageAsync(lastMsg);
                    }
                }
            }
        }

        public CalculationProgress GetProgress()
        {
            lock (_locker)
            {
                return new CalculationProgress(
                    _calculationGuid,
                    isFinished: _isFinished,
                    isCancelled: _isDisposed || _cancellationToken.IsCancellationRequested,
                    _calculatedList.Select(x => x.Value));
            }
        }

        private void OnMsgReceived(FiboMessage receivedMsg)
        {
            _logger?.LogDebug($"{nameof(FiboCalculation)} {_calculationGuid}. Msg received. {receivedMsg}");

            if (IsStopped)
            {
                return;
            }

            var isAdded = CheckAndAddNextMsg(receivedMsg);
            if (isAdded)
            {
                SendMessageAsync(receivedMsg);
            }
        }

        private FiboMessage GetInitialMessage()
        {
            return new FiboMessage
            {
                KeyId = _calculationId,
                PrevValue = 0,
                Value = 0,
                IterationIndex = 0,
            };
        }

        private async Task SendMessageAsync(FiboMessage msg, bool throwIfError = false)
        {
            if (IsStopped)
            {
                return;
            }

            try
            {
                var response = await _msgSender.SendMsg(msg, _cancellationToken);
                if (response.ok)
                {
                    return;
                }

                var statusCode = response.statusCode;
                if (IsSendMsgStatusCompleted(statusCode))
                {
                    _isFinished = true;
                    return;
                }

                _logger?.LogWarning($"REST-server call error: {response.message ?? ""}");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Cannot send REST message to server");
                if (throwIfError)
                {
                    throw;
                }
            }
        }

        private bool IsSendMsgStatusCompleted(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.UnprocessableEntity;
        }

        private bool CheckAndAddNextMsg(FiboMessage msgToAdd)
        {
            lock (_locker)
            {
                var lastMsg = _calculatedList.LastOrDefault();
                if (lastMsg != null
                    && msgToAdd.IterationIndex == lastMsg.IterationIndex + 1
                    && msgToAdd.KeyId == _calculationId
                )
                {
                    _calculatedList.Add(msgToAdd);
                    _hasProgressFromPreviousCheck = true;
                    return true;
                }
                return false;
            }
        }

        private bool IsStopped => _cancellationToken.IsCancellationRequested || _isDisposed || _isFinished;

        public void Dispose()
        {
            try
            {
                _bus.Advanced.Connected -= HandleOnConnected;
            }
            catch { }

            _isDisposed = true;
            _logger?.LogDebug($"Disposing {nameof(FiboCalculation)} {_calculationGuid}");
            _subscription?.Dispose();
            _subscription = null;
            _msgSender.Dispose();
        }
    }
}
