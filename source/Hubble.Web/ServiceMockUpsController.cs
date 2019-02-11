using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Hubble.Web
{
    [Route("mockups/{serviceName}/[action]")]
    [ApiController]
    public class ServiceMockUpsController : ControllerBase {                

        [HttpGet]
        public object Alerts(string serviceName) {
               var services = new [] {
                new {
                    Name = "apollo13",
                    Alerts = new[] {
                        new ServiceAlert{
                            Id = "OxygenLevel",
                            Raised = true,
                            Confirmed = false
                        }
                    }
                },
                new {
                    Name = "spaceshuttle",
                    Alerts = new[] {
                        new ServiceAlert{
                            Id = "HullIntegrity",
                            Raised = true,
                            Confirmed = true
                        },
                         new ServiceAlert{
                            Id = "FuelSupplyLow",
                            Raised = true,
                            Confirmed = false
                        }
                    }
                }                
            }.ToDictionary(x => x.Name);

            serviceName = serviceName?.ToLowerInvariant() ?? string.Empty;

            if (services.ContainsKey(serviceName)) {
                return services[serviceName].Alerts;
            }            
            
            return NoContent();
        }

        [HttpGet]
        public object Ping(string serviceName) {            
            var services = new [] {
                new {
                    Name = "apollo13",
                    Status = HttpStatusCode.Gone
                },
                new {
                    Name = "spaceshuttle",
                    Status = HttpStatusCode.OK
                }                
            }.ToDictionary(x => x.Name);
            
            serviceName = serviceName?.ToLowerInvariant() ?? string.Empty;

            if (services.ContainsKey(serviceName)) {
                return StatusCode((int)services[serviceName].Status);
            }
            
            return NotFound();
        }
    }
}
