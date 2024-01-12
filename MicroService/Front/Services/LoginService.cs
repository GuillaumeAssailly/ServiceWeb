using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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

        public async Task<UserDTO> AuthenticateUser(string username, string password)
        {

            UserLogin user = new UserLogin()
            {
                Name = username,
                Pass = password
            };
         
            var response = await _httpClient.PostAsJsonAsync("/api/User/login",user).ConfigureAwait(false) ;
            
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<UserDTO>();
                return result;
            }
             return null;
            
        }

        private string GenerateJwtToken(int userId)
        {
            var claims = new List<Claim>
            {
                // On ajoute un champ UserId dans notre token avec comme valeur userId en string
                new Claim("UserId", userId.ToString())
            };

            // On créer la clé de chiffrement
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyLongLongLongLongEnough"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // On paramètre notre token
            var token = new JwtSecurityToken(
                issuer: "TodoProject", // Qui a émit le token
                audience: "localhost:5000", // A qui est destiné ce token
                claims: claims, // Les données que l'on veux encoder dans le token
                expires: DateTime.Now.AddMinutes(3000), // Durée de validité
                signingCredentials: creds); // La clé de chiffrement

            // On renvoie le token signé
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

