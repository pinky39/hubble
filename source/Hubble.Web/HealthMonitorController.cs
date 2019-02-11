using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;

namespace Hubble.Web
{
    [Route("{serviceName}/[action]")]
    [ApiController]
    public class HealthMonitorController : ControllerBase
    {
        private Dictionary<string, RegisteredService> _registeredServices;
        private readonly ILogger<HealthMonitorController> _logger;

        public HealthMonitorController(IConfiguration cfg, ILogger<HealthMonitorController> logger)
        {
            _registeredServices = cfg.GetSection("RegisteredServices")
                 .Get<RegisteredService[]>()
                 .ToDictionary(x => x.Name.ToLowerInvariant());

            _logger = logger;
        }

        public StatusCodeResult HealthCheckFailed() => StatusCode((int)HttpStatusCode.ServiceUnavailable);

        [Throttle(Seconds = 5)]
        [HttpGet]
        public async Task<object> Ping(string serviceName)
        {
            serviceName = serviceName?.ToLowerInvariant() ?? string.Empty;

            RegisteredService serviceConfiguration;
            if (_registeredServices.TryGetValue(serviceName, out serviceConfiguration))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serviceConfiguration.Url);
                    try
                    {
                        var result = await client.GetAsync(serviceConfiguration.Ping);
                        if (result.IsSuccessStatusCode)
                        {
                            return Ok();
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogWarning(ex, $"Service unreachable ('{serviceName}').");
                        return HealthCheckFailed();
                    }
                }
            }
            else {
                _logger.LogWarning($"Service not configured ('{serviceName}')");
            }

            return HealthCheckFailed();
        }

        [Throttle(Seconds = 5)]
        [HttpGet("{alertName}")]
        [HttpGet("")]
        public async Task<object> Alerts(string serviceName, string alertName)
        {
            serviceName = serviceName?.ToLowerInvariant() ?? string.Empty;

            RegisteredService serviceConfiguration;
            if (_registeredServices.TryGetValue(serviceName, out serviceConfiguration))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serviceConfiguration.Url);

                    try
                    {
                        var result = await client.GetAsync(serviceConfiguration.Alerts);

                        if (!result.IsSuccessStatusCode)
                        {
                            return HealthCheckFailed();
                        }

                        string content = await result.Content.ReadAsStringAsync();
                        var alerts = JsonConvert.DeserializeObject<ServiceAlert[]>(content);

                        if (alerts == null)
                        {
                            if (content.Length > 0) {
                                _logger.LogWarning($"Invalid response ('{serviceName}').\n{content}");
                                
                                return HealthCheckFailed();
                            }
                                
                            return Ok();
                        }

                        var filtered = string.IsNullOrEmpty(alertName)
                            ? alerts
                            : alerts.Where(x => x.Id.Equals(alertName, StringComparison.InvariantCultureIgnoreCase))
                            .ToArray();

                        if (filtered.All(x => !x.Raised || x.Confirmed))
                            return Ok();
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogWarning(ex, $"Service unreachable ('{serviceName}').");
                        return HealthCheckFailed();
                    }
                }
            }
            else {
                _logger.LogWarning($"Service not configured ('{serviceName}')");
            }

            return HealthCheckFailed();
        }
    }
}
