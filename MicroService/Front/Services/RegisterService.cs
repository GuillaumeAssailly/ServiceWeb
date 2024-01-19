using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Ajoutez cette ligne pour utiliser ILogger
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;


namespace Front.Services
{
    public class RegisterService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RegisterService> _logger; // Ajoutez cette ligne

        public RegisterService(HttpClient httpClient, ILogger<RegisterService> logger) // Ajoutez ILogger dans le constructeur
        {
            _httpClient = httpClient;
            _logger = logger; // Ajoutez cette ligne
            _httpClient.BaseAddress = new System.Uri("http://localhost:5000");
        }
        public async Task<UserDTO> RegisterUser(string username, string password, string mail)
        {
            UserCreateModel user = new UserCreateModel()
            {
                Name = username,
                Password = password,
                Email = mail
            };

            var response = await _httpClient.PostAsJsonAsync("/api/User/register", user);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<UserDTO>();
                return result;
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (errorMessage.Contains("Email"))
                {
                    _logger.LogInformation("Email already exists");

                }
                else if (errorMessage.Contains("Username"))
                {
                    _logger.LogInformation("Username already exists");
                }
                else
                {
                    _logger.LogError("Registration failed due to conflict: " + errorMessage);
                }
            }

            return null;
        }
    }
}
