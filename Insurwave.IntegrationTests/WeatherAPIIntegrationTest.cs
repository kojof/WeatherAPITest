using System;
using System.Net.Http;
using System.Threading.Tasks;
using Insurwave.Domain.Entities;
using Insurwave.WebAPI;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Insurwave.IntegrationTests
{

    public class WeatherAPIIntegrationTest
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public WeatherAPIIntegrationTest()
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }



        [Fact]
        public async Task CanGetWeather()
        {
            // Act
            var response = await _client.GetAsync("/api/WeatherAPI/Get?city=london");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var weather = JsonConvert.DeserializeObject<Weather>(responseString);
            Console.WriteLine(responseString);
            // Assert
            Assert.Equal("london", weather.City);
        }
    }
}