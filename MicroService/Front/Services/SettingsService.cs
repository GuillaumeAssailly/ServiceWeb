using Front.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;


namespace Front.Services
{
    public class SettingsService
    {

        public List<UserDTO> Users;
        public List<Entry> History;
        public List<Entry> AllHistory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private ProtectedLocalStorage _localStorage;
        public SettingsService(AuthenticationStateProvider authenticationStateProvider, ProtectedLocalStorage localStorage)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }
        public async Task getUserList()
        {
            try
            {
                using (var httpClientUsers = new HttpClient())
                {
                    httpClientUsers.BaseAddress = new Uri("http://127.0.0.1:5001/");
                    Users = await httpClientUsers.GetFromJsonAsync<List<UserDTO>>("api/Users");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }

        public async Task RefreshUserList()
        {
            try
            {
                using (var httpClientUsers = new HttpClient())
                {
                    httpClientUsers.BaseAddress = new Uri("http://127.0.0.1:5001/");
                    Users = await httpClientUsers.GetFromJsonAsync<List<UserDTO>>("api/Users");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de la liste des utilisateurs : {ex.Message}");
            }
        }

        public async Task deleteUser(int id)
        {

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("http://127.0.0.1:5001/");

                    // Envoyer une requête DELETE à l'API pour supprimer l'utilisateur
                    HttpResponseMessage response = await httpClient.DeleteAsync($"api/Users/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        // La suppression a réussi
                        Console.WriteLine($"L'utilisateur avec l'ID {id} a été supprimé avec succès.");

                        // Rafraîchir la liste des utilisateurs après la suppression
                        await RefreshUserList();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // L'utilisateur n'a pas été trouvé
                        Console.WriteLine($"L'utilisateur avec l'ID {id} n'a pas été trouvé.");
                    }
                    else
                    {
                        // Une autre erreur s'est produite
                        Console.WriteLine($"Erreur lors de la suppression de l'utilisateur avec l'ID {id}. Statut : {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
        public async Task getHistory()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity.IsAuthenticated)
                {
                    // var userId = user.Identity.Name;
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier).Value;


                    if (userId != null)
                    {
                        using (var httpClientHistory = new HttpClient())
                        {
                            httpClientHistory.BaseAddress = new Uri("http://127.0.0.1:5097/");
                            History = await httpClientHistory.GetFromJsonAsync<List<Entry>>($"api/History/{userId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

        }

        public async Task getAllHistory()
        {
            try
            {
                using (var httpClientHistory = new HttpClient())
                {
                    httpClientHistory.BaseAddress = new Uri("http://127.0.0.1:5097/");
                    AllHistory = await httpClientHistory.GetFromJsonAsync<List<Entry>>($"api/History/all");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

        }

        public async Task deleteEntry(int id)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var jwtToken = await _localStorage.GetAsync<string>("jwt");
                    httpClient.BaseAddress = new Uri("http://127.0.0.1:5000");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.Value);
                    HttpResponseMessage response = await httpClient.DeleteAsync($"api/History/{id}");
                    await RafreshHistory();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
        }
        public async Task RafreshHistory()
        {
            await getAllHistory();
            await getHistory();
        }




    }
}
