using System;
using System.Globalization;
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
        private TemperatureMeasurement _temperatureMeasurement = TemperatureMeasurement.C;
        private double _temperatureFahrenheitMeasurement = 41;
        private double _temperatureCelciusMeasurement = 5.0;
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
        public async Task GetCurrentWeather_ValidInput_Return_ValidResult()
        {
            SetupHttpClient();
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);

            var response = await _sut.GetCurrentWeather(_city, _temperatureMeasurement);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.City, _city);
        }


        [TestMethod]
        public async Task GetCurrentWeather_ValidInput_With_CelciusTemperature_Return_ValidResult()
        {
            SetupHttpClient();
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);
            var response = await _sut.GetCurrentWeather(_city, TemperatureMeasurement.C);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.Temperature, _temperatureCelciusMeasurement);
        }

       

        [TestMethod]
        public async Task GetCurrentWeather_ValidInput_With_FahrenheitTemperature_Return_ValidResult()
        {
            SetupHttpClient();
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);

            var response = await _sut.GetCurrentWeather(_city, TemperatureMeasurement.F);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.Temperature, _temperatureFahrenheitMeasurement);
        }

        [TestMethod]
        public async Task GetCurrentWeather_InValidInput_Without_Temperature_Return_ValidResult()
        {
            SetupHttpClient();
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);
            var response = await _sut.GetCurrentWeather(_city, null);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.Temperature, _temperatureCelciusMeasurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Invalid Search Criteria.")]
        public async Task GetCurrentWeather_InValidInput_City_Return_ThrowsException()
        {
            SetupHttpClient();
            _sut = new WeatherAPIService(_logger.Object, _mockHttpClientFactory.Object, _configuration.Object);
            var response = await _sut.GetCurrentWeather("", null);
        }


        #region Private Methods
        private void SetupHttpClient()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(GetWeatherAPIResponse())),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            Uri weatherAPIBaseUrl = new Uri(_weatherAPIBaseUrl);

            _configuration.SetupGet(x => x[It.Is<string>(s => s == "WeatherAPIBaseUrl")])
                .Returns(weatherAPIBaseUrl.AbsoluteUri);
            _configuration.SetupGet(x => x[It.Is<string>(s => s == "WeatherAPIKey")])
                .Returns("b5acceae06b644e3a16224434211101");


            httpClient.BaseAddress = weatherAPIBaseUrl;
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
         
        }

        private WeatherAPIResponse GetWeatherAPIResponse()
        {
            var current = new Current
            {
                Temp_c = _temperatureCelciusMeasurement,
                Temp_f = _temperatureFahrenheitMeasurement
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

        #endregion
    }
}
