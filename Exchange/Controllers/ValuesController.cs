using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
namespace Exchange.Controllers
{

    [Route("api[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IMemoryCache _cache;
        //private MemoryCache _cache = MemoryCache.Default;
        private static readonly HttpClient HttpClient;
        
        public ValuesController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        
        static ValuesController()
        {
            HttpClient = new HttpClient();
        }

        [HttpGet("{from}/{to}/{value}")]
        public async Task<IActionResult> Get(string from, string to, double value)
        {
            double rateValue = await GetChangeValue(from, to);
            double result = rateValue * value;

            return Ok(result);
        }

        private async Task<double> GetChangeValue(string from, string to)
        {

            if (_cache.Get<string>(from).Count() == 0)
            {
                HttpResponseMessage response = await HttpClient.GetAsync($"https://api.exchangeratesapi.io/latest?base={from}&symbols={to}");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set(from, responseBody, cacheEntryOptions);
            }

            var item = _cache.Get<string>(from);
        
            JObject json = JObject.Parse(item);
            var rate = json["rates"][to].ToString();

            return Double.Parse(rate);
        }


    }
}
