#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Insurwave.Domain.Entities;
using Insurwave.Domain.Enums;
using Insurwave.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#endregion

namespace Insurwave.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherAPIController : ControllerBase
    {
        private readonly ILogger<WeatherAPIController> _logger;
        private readonly IWeatherAPIService _weatherAPIService;

        public WeatherAPIController(ILogger<WeatherAPIController> logger, IWeatherAPIService weatherAPIService)
        {
            _logger = logger;
            _weatherAPIService = weatherAPIService;
        }

       
        [HttpGet()]
        [Route("GetCurrentWeather")]
        public async Task<IActionResult> GetCurrentWeather(string city, TemperatureMeasurement? temperatureMeasurement = TemperatureMeasurement.C)
        {

            if (string.IsNullOrEmpty(city))
            {
                throw new ArgumentNullException(nameof(city));
            }

            try
            {
                var response = await _weatherAPIService.GetCurrentWeather(city, temperatureMeasurement);
                return Ok(response);
            }

            catch (HttpRequestException httpRequestException)
            {
                _logger.LogError("Error in GetCurrentWeather method: ", httpRequestException.StackTrace);
                return BadRequest($"Error getting weather from WeatherAPI: {httpRequestException.Message}");
            }

            return NoContent();
        }
    }
}
