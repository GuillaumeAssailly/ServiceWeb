using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace Front.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;
        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new System.Uri("http://localhost:5000");
        }

        public async Task<JWTAndUser> AuthenticateUser(string username, string password)
        {

            UserLogin user = new UserLogin()
            {
                Name = username,
                Pass = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/User/login",user).ConfigureAwait(false) ;
            //var response = await _httpClient.PostAsJsonAsync("/api/User/login",user) ;


            if (response.StatusCode == HttpStatusCode.OK)
            {
                

                var result = await response.Content.ReadFromJsonAsync<JWTAndUser>();
                //var jwtToken = result.Token;
                //Console.WriteLine($"jwt token into localStorage [{jwtToken}");
                //await _localStorage.SetItemAsStringAsync("jwt", jwtToken);
                //var jwtQuoted = await _localStorage.GetItemAsStringAsync("jwt");
                //Console.WriteLine($"localstorage LOGIN service: {jwtQuoted}");
                return result;
            }
             return null;
            
        }

    }
}

