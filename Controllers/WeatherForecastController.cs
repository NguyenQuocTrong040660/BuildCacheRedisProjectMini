using BuildCacheRedisProjectMini.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace BuildCacheRedisProjectMini.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IRedisCacheService _redisCacheService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IRedisCacheService IRedisCacheService)
        {
            _logger = logger;
            _redisCacheService = IRedisCacheService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// Thiết lập Cache Key 
        /// </summary>
        /// <param name="cacheItem"></param>
        /// <returns></returns>
        [HttpPost("setCacheKey")]
        [ProducesResponseType(StatusCodes.Status200OK)]  // Ghi rõ HTTP 200 khi thành công
        [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Ghi rõ HTTP 400 khi đầu vào không hợp lệ
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetKey([FromBody] CacheItem cacheItem)
        {
            try
            {
                if (cacheItem == null || string.IsNullOrEmpty(cacheItem.Key))
                {
                    return BadRequest("Invalid cache item.");
                }
                await _redisCacheService.SetCacheValueAsync(cacheItem.Key, cacheItem);
                // Trả về đối tượng CacheItem (bao gồm thời gian hết hạn)
                return Ok(new CacheItem
                {
                    Key = cacheItem.Key,
                    Value = cacheItem.Value,
                    ExpirationMinutes = cacheItem.ExpirationMinutes > 0 ? cacheItem.ExpirationMinutes : 60
                });
            }
            catch (Exception ex)
            {
                // Trả về HTTP 500 khi có lỗi hệ thống
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while setting the cache item.",
                    Details = ex.Message
                });
            }

        }






        /// <summary>
        /// Get 1 Cache Key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("get-key")]
        [ProducesResponseType(StatusCodes.Status200OK)]  // Ghi rõ HTTP 200 khi thành công
        [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Ghi rõ HTTP 400 khi đầu vào không hợp lệ
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetKey([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Key is required.");
            }

            var value = await _redisCacheService.GetCacheValueAsync<CacheItem>(key);

            return Ok(value);
        }



        /// <summary>
        /// Kiểm tra CacheKey có tồn tại không
        /// </summary>
        [HttpGet("existCacheKey")]
        [ProducesResponseType(StatusCodes.Status200OK)]  // Ghi rõ HTTP 200 khi thành công
        [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Ghi rõ HTTP 400 khi đầu vào không hợp lệ
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> existCacheKey([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Key is required.");
            }

            var value = await _redisCacheService.GetCacheValueAsync<CacheItem>(key);

            return Ok(value);
        }

        /// <summary>
        /// Xóa 1 CacheKey 
        /// </summary>
        [HttpGet("removeCacheKeyAsyn")]
        [ProducesResponseType(StatusCodes.Status200OK)]  // Ghi rõ HTTP 200 khi thành công
        [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Ghi rõ HTTP 400 khi đầu vào không hợp lệ
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> removeCacheKeyAsyn([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Key is required.");
            }

            var value = await _redisCacheService.GetCacheValueAsync<CacheItem>(key);

            return Ok(value);


        }


        [HttpPost("set")]
        public async Task<IActionResult> SetCache([FromBody] CacheRequest request)
        {
            if (string.IsNullOrEmpty(request.Key) || request.Data == null)
            {
                return BadRequest(new { Message = "Key and Data are required." });
            }

            try
            {
                // Tạo CacheKey từ request
                var cacheKey = new CacheKey(request.Key)
                {
                    CacheTime = request.CacheTime > 0 ? request.CacheTime : 60 // CacheTime mặc định là 60 phút nếu không cung cấp
                };

                // Gọi phương thức Set để lưu vào Redis
                await _redisCacheService.Set(cacheKey, request.Data);

                return Ok(new { Message = "Data cached successfully.", Key = request.Key });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode(500, new { Message = "An error occurred while setting the cache.", Details = ex.Message });
            }
        }
    }

  

 


        public class CacheItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public int ExpirationMinutes { get; set; }
        }

        public class CacheRequest
        {
            public string Key { get; set; }          // Key của cache
            public object Data { get; set; }         // Dữ liệu cần lưu
            public int CacheTime { get; set; } = 60; // Thời gian cache (mặc định 60 phút)
        }
  
}
