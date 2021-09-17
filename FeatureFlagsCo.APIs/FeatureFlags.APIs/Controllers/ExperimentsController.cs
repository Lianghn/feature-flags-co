using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExperimentsController : ControllerBase
    {
        private readonly ILogger<ExperimentsController> _logger;
        private readonly IEnvironmentService _envService;
        private readonly IExperimentsService _experimentsService;

        public ExperimentsController(
            ILogger<ExperimentsController> logger,
            IEnvironmentService envService,
            IExperimentsService experimentsService)
        {
            _logger = logger;
            _envService = envService;
            _experimentsService = experimentsService;
        }

        [HttpGet]
        [Route("CustomEvents/{envId}")]
        public async Task<dynamic> GetCustomEvents(int envId)
        {
            var currentUserId = this.HttpContext.User.Claims.FirstOrDefault(p => p.Type == "UserId").Value;
            if (await _envService.CheckIfUserHasRightToReadEnvAsync(currentUserId, envId))
            {
               return await _experimentsService.GetEnvironmentEvents(envId, MetricTypeEnum.CustomEvent);
            }

            return new List<string>();
        }
    }
}
