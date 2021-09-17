using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IExperimentsService
    {
        Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType);
    }

    public class ExperimentsService: IExperimentsService
    {
        private readonly IOptions<MySettings> _mySettings;

        public ExperimentsService(
            IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
        }

        public async Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType)
        {
            string esHost = _mySettings.Value.ElasticSearchHost;
            string indexTarget = "experiments";

            var termEO = new ExpandoObject();
            termEO.TryAdd("Type", metricType.ToString().ToLower());

            var body = new
            {
                query = new
                {
                    term = termEO
                }
            };

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                if (esHost.Contains("@")) // esHost contains username and password 
                {
                    var startIndex = esHost.LastIndexOf("/") + 1;
                    var endIndex = esHost.LastIndexOf("@");
                    var credential = esHost.Substring(startIndex, endIndex - startIndex).Split(":");
                    var userName = credential[0];
                    var password = credential[1];

                    esHost = esHost.Substring(0, startIndex) + esHost.Substring(endIndex + 1);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));
                }

                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/{indexTarget}/_search?size=0", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                return null;
            }
        }
    }
}
