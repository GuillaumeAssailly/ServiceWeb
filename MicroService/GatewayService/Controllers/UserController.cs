using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace GatewayService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // api/User/login
        //public async Task<ActionResult<UserDTO>> Login(UserLogin model)
        [HttpPost("login")]
        public async Task<ActionResult<JWTAndUser>> Login(UserLogin model)
        {
            // Create an HttpClient instance using the factory
            using (var client = _httpClientFactory.CreateClient())
            {
                // Set the base address of the API you want to call
                client.BaseAddress = new System.Uri("http://localhost:5001/");

                // Send a POST request to the login endpoint
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Users/login", model);

                // Check if the response status code is 200 (OK)
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // You can deserialize the response content here if needed
                    var result = await response.Content.ReadFromJsonAsync<UserDTO>();
                    var jwt = GenerateJwtToken(result.Id);
                    var userAndToken = new JWTAndUser { Token = jwt, User = result };
                    return Ok(userAndToken);
                }
                else
                {
                    return BadRequest("Login failed");
                }
            }
        }

        // api/User/register
        /*
        public async Task<ActionResult<UserDTO>> Register(UserCreateModel model)
        */

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateModel model)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Name, "^[a-zA-Z0-9]+$") || !System.Text.RegularExpressions.Regex.IsMatch(model.Password, "^[a-zA-Z0-9]+$"))
            {
                return BadRequest("Les credentials ne doivent contenir que des caractères alphanumériques.");
            }
            // Create an HttpClient instance using the factory
            using (var client = _httpClientFactory.CreateClient())
            {
                // Set the base address of the API you want to call
                client.BaseAddress = new System.Uri("http://localhost:5001/");

                // Send a POST request to the login endpoint
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Users/register", model);

                // Check if the response status code is 200 (OK)
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    // You can deserialize the response content here if needed
                    var result = await response.Content.ReadFromJsonAsync<UserDTO>();
                    return Ok(result);
                }
                else if(response.StatusCode == HttpStatusCode.Conflict)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Conflict(errorMessage);
                }
                else 
                {
                    return BadRequest("Register failed");
                }
            }
        }

        [Authorize]
        [HttpGet("jwt")]
        public ActionResult<string> Jwt()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

            foreach (var claim in User.Claims)
            {
                Console.WriteLine(claim.Type + " " + claim.Value);
            }
            Console.WriteLine("jwt");
            return Ok($"Hello, {userName}");
        }
        private string GenerateJwtToken(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyLongLongLongLongEnough"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TodoProject",
                audience: "localhost:5000",
                claims: claims,
                expires: DateTime.Now.AddMinutes(3000),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
