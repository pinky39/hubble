using System;
using Xunit;
using Hubble.Web;
using Xunit.Abstractions;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Threading;

namespace Hubble.Tests
{
    public class HubbleTests : IDisposable
    {
        private  IDisposable _server;
        private const string BaseAddress = "http://localhost:5000/";

        public HubbleTests()
        {                                                            
            var contentRoot = Path.Combine(
                System.AppContext.BaseDirectory, 
                @"..\..\..\..\Hubble.Web");            
            
            _server = new HubbleServer(contentRoot).Start();
        }

        public void Dispose()
        {
            _server?.Dispose();
        }

        [Fact]
        public async Task Ping_Ok()
        {
            await AssertStatus("spaceshuttle/ping", HttpStatusCode.OK);                          
        }

        [Fact]
        public async Task Ping_Throttle()
        {
            await AssertStatus("spaceshuttle/ping", HttpStatusCode.OK);                          
            await AssertStatus("spaceshuttle/ping", HttpStatusCode.Conflict);  
        }

        [Fact]
        public async Task Ping_Fail() {
            await AssertStatus("apollo13/ping", HttpStatusCode.ServiceUnavailable);                          
        }  

        
        [Fact]
        public async Task Ping_Unreachable() {
            await AssertStatus("opportunity/ping", HttpStatusCode.ServiceUnavailable);                          
        }  

        [Fact]      
        public async Task Alert_Ok() {
            await AssertStatus("spaceshuttle/alerts/HullIntegrity", HttpStatusCode.OK);                          
        }

        [Fact]      
        public async Task Alert_Fail() {
            await AssertStatus("apollo13/alerts/OxygenLevel", HttpStatusCode.ServiceUnavailable);                          
        }

         [Fact]      
         public async Task Alerts_Fail() {
            await AssertStatus("spaceshuttle/alerts", HttpStatusCode.ServiceUnavailable);                          
        }

        private async Task AssertStatus(string url, HttpStatusCode expected) {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                HttpResponseMessage result = await client.GetAsync(url);
                string content = await result.Content.ReadAsStringAsync();
                Assert.Equal(expected, result.StatusCode);                
            }
        }
    }
}
