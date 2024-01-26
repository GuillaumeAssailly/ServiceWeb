using GatewayService.Entities;
using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;

namespace GatewayService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private JwtService _jwtService;
        public HistoryController(IHttpClientFactory httpClientFactory, JwtService jwtService)
        {
            _httpClientFactory = httpClientFactory;
            _jwtService = jwtService;
        }

        private bool CheckJWT()
        {
            var token = HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last().Trim('"');
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("no token");
                return false;
            }
            try
            {
                Console.WriteLine($"token: {token}");
                ClaimsPrincipal principal = _jwtService.ValidateToken(token);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid token: {ex.Message}");
                return false;
            }

        }

        [HttpPost("add")]
        public async Task<IActionResult> AddHistoryEntry(Entry historyEntry)
        {
            if (!CheckJWT()) return BadRequest();

            using (var client = _httpClientFactory.CreateClient())
            {
                // Set the base address of the API you want to call
                client.BaseAddress = new System.Uri("http://localhost:5097/");
                HttpResponseMessage response = await client.PostAsJsonAsync("api/History/add", historyEntry);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("added success ! ");
                    return Ok();
                }
                else
                {
                    Console.WriteLine("added failed !");
                    return BadRequest();
                }
            }

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            if (!CheckJWT()) return BadRequest();
            Console.WriteLine("Token OK ! ");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("http://127.0.0.1:5097");
                    HttpResponseMessage response = await httpClient.DeleteAsync($"api/History/{id}");
                    Console.WriteLine("Delete finished");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllHistoriesEntry()
        {
            if (!CheckJWT()) return BadRequest();
            try
            {
                using (var httpClient = new HttpClient())
                {

                    httpClient.BaseAddress = new Uri("http://127.0.0.1:5097");
                    List<Entry> response = await httpClient.GetFromJsonAsync<List<Entry>>($"api/History/all");
                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHistoryEntriesById(int id)
        {
            if (!CheckJWT()) return BadRequest();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("http://127.0.0.1:5097");
                    List<Entry> response = await httpClient.GetFromJsonAsync<List<Entry>>($"api/History/{id}");
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return BadRequest();
            }
        }
    }
}
