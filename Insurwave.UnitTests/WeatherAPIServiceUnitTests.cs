using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Insurwave.Application.Services;
using Insurwave.Domain.Entities;
using Insurwave.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Insurwave.UnitTests
{
    [TestClass]
    public class WeatherAPIServiceUnitTests
    {
        #region Private Variables

        private WeatherAPIService _sut;
        private Mock<ILogger<WeatherAPIService>> _logger;
        private Mock<IConfiguration> _configuration;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private string _city = "london";
        private string _weatherAPIBaseUrl = "https://api.weatherapi.com/v1/";
        private string _weatherAPIKey;

        #endregion

        public WeatherAPIServiceUnitTests()
        {
            _logger = new Mock<ILogger<WeatherAPIService>>();
            _configuration = new Mock<IConfiguration>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        }

        [TestInitialize]
        public void Init()
        {
          
        }

        [TestMethod]
        public async Task GetCurrentWeather_ValidInput_ReturnValidResult()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(GetResponse())),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            Uri weatherAPIBaseUrl = new Uri(_weatherAPIBaseUrl);

            _configuration.SetupGet(x => x[It.Is<string>(s => s == "WeatherAPIBaseUrl")])
                .Returns(weatherAPIBaseUrl.AbsoluteUri);
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "WeatherAPIKey")])
                .Returns("b5acceae06b644e3a16224434211101");

            
            httpClient.BaseAddress = weatherAPIBaseUrl;
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);

            var response = await _sut.GetCurrentWeather(_city);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.City, _city);
        }


        private WeatherAPIResponse GetResponse()
        {
            var current = new Current
            {
                Temp_c = 5.0,
                Temp_f = 41
            };

            var location = new Location
            {
                Name = _city,
                Country = "United Kingdom",
                Localtime = "2021-01-17 22:33",
                Region = "City of London, Greater London"
            };

            var weatherAPIResponse = new WeatherAPIResponse()
            {
                Current = current,
                Location = location
            };

            return weatherAPIResponse;
        }
    }
}
