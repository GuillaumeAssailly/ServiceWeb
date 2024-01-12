using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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
            _httpClient.BaseAddress = new System.Uri("http://localhost:5001");
        }

        public async Task<UserDTO> RegisterUser(string username, string password)
        {

            UserCreateModel user = new UserCreateModel()
            {
                Name = username,
                Password = password,
                Email = ""
            };
         
            var response = await _httpClient.PostAsJsonAsync("/api/Users/register",user) ;
            
            if(response.StatusCode == HttpStatusCode.Created)
            {
                var result = await response.Content.ReadFromJsonAsync<UserDTO>();
                return result;
            }
            return null;
            
        }
    }


}
