using Front.Entities;
using System.Net;


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

        public async Task<UserDTO> RegisterUser(string username, string password, string mail)
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
                var result = await response.Content.ReadFromJsonAsync<UserDTO>();
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
