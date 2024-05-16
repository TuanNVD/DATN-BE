using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TilesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TilesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet("/tile/{z}/{x}/{y}.png")]
        public async Task<IActionResult> GetImage(int z, int x, int y)
        {
            var imageUrl = $"http://172.29.111.94/tile/{z}/{x}/{y}.png";

            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var response = await httpClient.GetAsync(imageUrl);

                if (response.IsSuccessStatusCode)
                {
                    var imageStream = await response.Content.ReadAsStreamAsync();
                    return File(imageStream, "image/png");
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}
