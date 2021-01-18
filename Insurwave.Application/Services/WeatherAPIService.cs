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

        public async Task<Weather> GetCurrentWeather(string city, TemperatureMeasurement? temperatureMeasurement)
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
                    response = await GetCurrentWeather(city, temperatureMeasurement, httpClient);

                    if (response != null)
                    {
                        var astronomyWeatherResponse = await GetAstronomyWeather(city, httpClient);

                        if (astronomyWeatherResponse != null)
                        {
                            response.Sunset = astronomyWeatherResponse.Sunrise;
                            response.Sunrise = astronomyWeatherResponse.Sunset;
                            return response;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return response;
        }
    


    private async Task<Weather> GetAstronomyWeather(string city, HttpClient httpClient)
    {
        Weather weather = null;
            using (HttpResponseMessage response = await httpClient.GetAsync($"astronomy.json?key={_weatherAPIKey}&q={city}"))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var astronomyResult = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<WeatherAPIResponse>(astronomyResult);
                    if (result != null)
                        weather = new Weather()
                        {
                            Sunrise = result.Astronomy.Astro.Sunrise,
                            Sunset = result.Astronomy.Astro.Sunset
                        };
                    return weather;
                }
            }
            return weather;
        }

        private async Task<Weather> GetCurrentWeather(string city, TemperatureMeasurement? temperatureMeasurement, HttpClient httpClient)
        {
            Weather weather = null;
            using (HttpResponseMessage currentResponse = await httpClient.GetAsync($"current.json?key={_weatherAPIKey}&q={city}"))
            {
                if (currentResponse.StatusCode == HttpStatusCode.OK)
                {
                    double temperature;
                    var currentWeatherResult = await currentResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<WeatherAPIResponse>(currentWeatherResult);

                    if (result != null)
                    {
                        temperature = GetTemperatureMeasurement(temperatureMeasurement, result.Current);

                        weather = new Weather()
                        {
                            City = result.Location.Name,
                            Region = result.Location.Region,
                            Country = result.Location.Country,
                            LocalTime = result.Location.Localtime,
                            Temperature = temperature
                        };
                        return weather;
                    }
                }
            }
            return weather;
        }

        private static double GetTemperatureMeasurement(TemperatureMeasurement? temperatureMeasurement, Current result)
        {
            double temperature;

            switch (temperatureMeasurement)
            {
                case TemperatureMeasurement.C:
                     temperature = result.Temp_c;
                    break;

                case TemperatureMeasurement.F:
                    temperature = result.Temp_f;
                    break;

                default:
                    temperature = result.Temp_c;
                    break;
            }

            return temperature;
        }
    }
}
