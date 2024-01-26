using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

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
                    var jwt = GenerateJwtToken(result);
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

        private string? GenerateJwtToken(UserDTO userDto)
        {
            var secretKey = "3f8aba3c3cfaa6ac99a153834438bc43e595e62c59c4385b4c1f9e31ed495eaa";
            var issuer = "Issuer";
            var audience = "localhost:5000";

            if (secretKey == null) return null;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            if (userDto.Name == null) return null;
            var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Email, userDto.Email),
            new ("name", userDto.Name),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, userDto.Id.ToString())
        };

            
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
    }
}
