using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;


namespace Front.Services
{
    public class RegisterService
    {
        private readonly HttpClient _httpClient;

        public RegisterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new System.Uri("http://localhost:5000");
        }

        public async Task<JWTAndUser> RegisterUser(string username, string password, string mail)
        {

            UserCreateModel user = new UserCreateModel()
            {
                Name = username,
                Password = password,
                Email = mail
            };

            var response = await _httpClient.PostAsJsonAsync("/api/User/register", user);
            
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<JWTAndUser>();
                return result;
            } else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                if (errorMessage.Contains("Email"))
                {
                    Console.WriteLine("Email already exists");
                }
                else if (errorMessage.Contains("Username"))
                {

                    Console.WriteLine("Username already exists");
                }
                else
                {
                    Console.WriteLine("Registration failed due to conflict: " + errorMessage);
                }
            }
            return null;
            
        }
    }
}
