using Fibo.FiboServer.App.Config;
using Fibo.FiboServer.App.Services;
using Fibo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace Fibo.FiboServer.App.Controllers
{
    [Route("api/fibo")]
    [ApiController]
    public class FiboController : ControllerBase
    {
        private readonly ILogger<FiboController> _logger;
        private readonly IFiboService _fiboService;
        private readonly ServicesConfiguration _configuration;

        public FiboController(
            ILogger<FiboController> logger,
            IFiboService fiboService,
            IOptions<ServicesConfiguration> configuration
        )
        {
            _logger = logger;
            _fiboService = fiboService;
            _configuration = configuration.Value;
        }

        /// <summary>
        /// Calculates next Fibonacci value (Int64) and redirects for next processing
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// Status 200 - ok, calculated and successfully sent for next processing.
        /// 400 - Bad input values
        /// 422 - Too large numeric value requested. Need to stop processing.
        /// 424 - Failed sending messasge for next processing.
        /// </returns>
        [HttpPost("next")]
        public async Task<ActionResult> Next([FromBody] FiboMessage request)
        {
            if (request.PrevValue < 0)
            {
                return base.BadRequest($"Bad {nameof(FiboMessage.PrevValue)} value");
            }

            if (request.Value < 0)
            {
                return base.BadRequest($"Bad {nameof(FiboMessage.Value)} value");
            }

            FiboMessage nextMessage;
            try
            {
                nextMessage = _fiboService.CalculateNextMessage(request);
            }
            catch (OverflowException)
            {
                return base.StatusCode((int)HttpStatusCode.UnprocessableEntity, "Too large value requested");
            }

            try
            {
                if (_configuration.SimulateDelay)
                {
                    var min = _configuration.DelayMinMs >= 0 ? _configuration.DelayMinMs : 300;
                    var max = _configuration.DelayMaxMs >= 0 ? _configuration.DelayMaxMs : 800;
                    await Task.Delay(new Random().Next(min, max));
                }
                _fiboService.SendMessage(nextMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed send message to next processing via Message Bus", request);
                return base.StatusCode((int)HttpStatusCode.FailedDependency, "Failed send message to next processing via Message Bus");
            }

            return Ok("Successfully sent to next processing via Message Bus");
        }
    }
}
