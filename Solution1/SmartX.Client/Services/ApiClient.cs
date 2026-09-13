//This  tells forms how to construct HTTP requests
using SmartX.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SmartX.Client.Services
{
    internal class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient()
        {
            _http = new HttpClient
            {
                BaseAddress =  new Uri("http://localhost:5056")
            };
        }

        public async Task <Sensor?> RegisterSensorAsync(Sensor sensor)
        {
            var response = await _http.PostAsJsonAsync("/sensors", sensor);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Sensor>();
        }

        public async Task<List<Sensor>> GetAllSensorsAsync()
        {
            var sensors = await _http.GetFromJsonAsync<List<Sensor>>("/sensors");
            return sensors ?? new List<Sensor>();
        }

    }
}
