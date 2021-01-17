using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Insurwave.Domain.Entities;
using Insurwave.Domain.Enums;

namespace Insurwave.Domain.Services
{
    public interface IWeatherAPIService
    {
        Task<Weather> GetCurrentWeather(string city, TemperatureMeasurement? temperatureMeasurement);
    }
}
