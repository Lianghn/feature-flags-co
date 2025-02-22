﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FeatureFlags.APIs.Authentication;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Repositories;
using FeatureFlags.APIs.Services;
using FeatureFlags.APIs.ViewModels;
using FeatureFlagsCo.FeatureInsights;
using FeatureFlagsCo.MQ;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Controllers
{
    //[Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _redisCache;
        private readonly IInsighstMqService _rabbitmqInsightsService;
        private readonly INoSqlService _noSqlService;
        private readonly IFeatureFlagsUsageService _ffUsageService;
        private readonly IOptions<MySettings> _mySettings;

        //private readonly ILaunchDarklyService _ldService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IDistributedCache redisCache,
            IInsighstMqService rabbitmqInsightsService,
            INoSqlService noSqlService,
            IOptions<MySettings> mySettings,
            IFeatureFlagsUsageService ffUsageService)
        {
            _logger = logger;
            _redisCache = redisCache;
            _rabbitmqInsightsService = rabbitmqInsightsService;
            _noSqlService = noSqlService;
            _ffUsageService = ffUsageService;
            _mySettings = mySettings;
        }

        [HttpGet]
        [Route("CreateMockMongoFeatureFlag")]
        public async Task<JsonResult> CreateMockMongoFeatureFlag()
        {
            var guid = Guid.NewGuid().ToString();
            FeatureFlag newFF = null;
            try
            {
                newFF = await _noSqlService.CreateFeatureFlagAsync(new CreateFeatureFlagViewModel
                {
                    CreatorUserId = "",
                    EnvironmentId = -1,
                    KeyName = guid,
                    Name = guid,
                    Status = FeatureFlagStatutEnum.Enabled.ToString(),
                    Id = FeatureFlagKeyExtension.GetFeatureFlagId(guid, "-1")
                }, "", -1, -1);
            }
            catch(Exception)
            {
                new Exception("mock test erreur");
            }
            return new JsonResult(newFF);
        }

        [HttpPost]
        [Route("redistest")]
        public string RedisTest([FromBody] GetUserVariationResultParam param)
        {
            var date = Guid.NewGuid().ToString() + DateTime.UtcNow.ToString();
            var serializedParam = JsonConvert.SerializeObject(param);
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = param.FeatureFlagKeyName,
                ["accountId"] = param.FeatureFlagKeyName,
                ["projectId"] = param.FeatureFlagKeyName,
                ["userKeyName"] = param.FeatureFlagKeyName,
                ["serializedParam"] = serializedParam
            };
            _redisCache.SetString(date, serializedParam);
            using (_logger.BeginScope(customizedTraceProperties))
            {
                _logger.LogInformation("variation-wr-request");
                //_logger.LogWarning("variation-wr-request");
            }
            return _redisCache.GetString(date);
        }

        [HttpPost]
        [Route("throwexception")]
        public string ThrowException([FromBody] GetUserVariationResultParam param)
        {
            throw new Exception("ThrowException test");
        }

        [HttpGet]
        [Route("ReturnTest200")]
        public async Task<JsonResult> ReturnTest200()
        {
            var date = Guid.NewGuid().ToString();
            var customizedTraceProperties = new Dictionary<string, object>()
            {
                ["envId"] = "param.FeatureFlagKeyName",
                ["accountId"] = "param.FeatureFlagKeyName",
                ["projectId"] = "param.FeatureFlagKeyName",
                ["userKeyName"] = "param.FeatureFlagKeyName",
                ["serializedParam"] = "serializedParam"
            };
            _redisCache.SetString(date, JsonConvert.SerializeObject(customizedTraceProperties));
            _redisCache.GetString(date);
            Response.StatusCode = 200;
            await _rabbitmqInsightsService.SendMessageAsync(new FeatureFlagsCo.MQ.MessageModel
            {
                Labels = new List<FeatureFlagsCo.MQ.MessageLabel>()
                 {
                     new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "email", LabelValue = "hu-beau@outlook.com"},
                     new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "timestamp", LabelValue = DateTime.UtcNow.ToString()}
                 },
                Message = "Very very Very very Very very Very very Very very Very very long message.",
                SendDateTime = DateTime.UtcNow
            });
            return new JsonResult(new VariationOption());
        }

        [HttpPost]
        [Route("SendDataToElasticSearch")]
        public void SendDataToElasticSearch()
        {
            var date = Guid.NewGuid().ToString();
            Response.StatusCode = 200;
            var testFeatureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(date, "-1");
            for(int i = 1; i > 0; i--)
            {
                _rabbitmqInsightsService.SendMessage(new FeatureFlagsCo.MQ.MessageModel
                {
                    Labels = new List<FeatureFlagsCo.MQ.MessageLabel>()
                     {
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "dataType", LabelValue = $"SendDataToElasticSearchTest"},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "email", LabelValue = $"hu-beau-{i}@outlook.com"},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "TimeStamp", LabelValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff")},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "customizedProperty1", LabelValue = i.ToString()},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "featureflagId", LabelValue = "FF__-1__-1__-1__e3fa98ec-6aaa-426a-a870-8eb3847b0233"},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "VariationLocalId", LabelValue = i.ToString()},
                         new FeatureFlagsCo.MQ.MessageLabel{ LabelName = "VariationValue", LabelValue = "false"},
                    },
                    Message = "",
                    SendDateTime = DateTime.UtcNow,
                    FeatureFlagId = "FF__-1__-1__-1__e3fa98ec-6aaa-426a-a870-8eb3847b02e3",
                    IndexTarget = "ffvariationrequestindex"
                });
            }
        }

        [HttpGet]
        [Route("GetLinearUsageCountByTimeRange")]
        public async Task<string> GetLinearUsageCountByTimeRange()
        {
            return await _ffUsageService.GetLinearUsageCountByTimeRangeAsync(_mySettings.Value.ElasticSearchHost,
                                                                             "ffvariationrequestindex",
                                                                             "FF__-1__-1__-1__e3fa98ec-6aaa-426a-a870-8eb3847b0233", 
                                                                             DateTime.UtcNow.AddHours(-5),
                                                                             DateTime.UtcNow,
                                                                             10);
        }
        

        [HttpGet]
        [Route("ReturnTest501")]
        public JsonResult ReturnTest501()
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new JsonResult(new VariationOption());
        }

        [HttpGet]
        [Route("redistest2")]
        public string RedisTest2()
        {
            return "true";
        }


        [HttpGet]
        [Route("probe")]
        public IActionResult Probe()
        {
            return Ok();
        }
    }
}
