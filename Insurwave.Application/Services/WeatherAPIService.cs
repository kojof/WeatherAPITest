using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Insurwave.Domain.Entities;
using Insurwave.Domain.Enums;
using Insurwave.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Insurwave.Application.Services
{
    public class WeatherAPIService : IWeatherAPIService
    {
        //    private readonly HttpClient _httpClient;
        //  static readonly HttpClient _httpClient = new HttpClient();
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<WeatherAPIService> _logger;
        private readonly IConfiguration _configuration;
        private string _weatherAPIBaseUrl;
        private string _weatherAPIKey;


        public WeatherAPIService(ILogger<WeatherAPIService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _weatherAPIBaseUrl = _configuration["WeatherAPIBaseUrl"];
            _weatherAPIKey = _configuration["WeatherAPIKey"];
        }

        public async Task<Weather> GetCurrentWeather(string city)
        {
            Weather response = null;

            if (string.IsNullOrEmpty(city))
            {
                throw new ArgumentNullException(nameof(city));
            }

            try
            {
                using (HttpClient httpClient = _httpClientFactory.CreateClient())
                {
                    httpClient.BaseAddress = new Uri(_weatherAPIBaseUrl);
                    response = await GetCurrentWeather(city, httpClient);
                }
            }

            catch (Exception ex)
            {
              _logger.LogError(ex, ex.Message);
            }

            return response;
        }

        private async Task<Weather> GetCurrentWeather(string city, HttpClient httpClient)
        {
            Weather weather = null;
            using (HttpResponseMessage currentResponse = await httpClient.GetAsync($"current.json?key={_weatherAPIKey}&q={city}"))
            {
                if (currentResponse.StatusCode == HttpStatusCode.OK)
                {
                    var currentWeatherResult = await currentResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<WeatherAPIResponse>(currentWeatherResult);

                    if (result != null)
                    {

                        weather = new Weather()
                        {
                            City = result.Location.Name,
                            Region = result.Location.Region,
                            Country = result.Location.Country,
                            LocalTime = result.Location.Localtime,
                            Temperature = result.Current.Temp_c
                        };
                        return weather;
                    }
                }
            }
            return weather;
        }

    }
}
