using Front.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NuGet.Protocol;
using System.Security.Claims;
using System.Security.Principal;

namespace Front.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        private ProtectedLocalStorage _sessionStorage;

        public CustomAuthenticationStateProvider(ProtectedLocalStorage protectedSessionStorage)
        {
            _sessionStorage = protectedSessionStorage;
        }

        public async Task<ClaimsPrincipal> MarkUserAsAuthenticated(JWTAndUser user)
        {
            await _sessionStorage.SetAsync("jwt", user.Token);
            await _sessionStorage.SetAsync("User", user.User);
            //var jwtToken = await _sessionStorage.GetAsync<string>("jwt");
            //Console.WriteLine($"UserName : {userSession.Value.Name}");
            //Console.WriteLine($"JWT Token : {jwtToken.Value}");

            var claims = new[] {
                new Claim(ClaimTypes.Name, user.User.Name),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.NameIdentifier, user.User.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return _currentUser;
        }
        public async Task<ClaimsPrincipal> Logout()
        {
            await _sessionStorage.DeleteAsync("User");
            await _sessionStorage.DeleteAsync("jwt");
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return _currentUser;
        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userSession = await _sessionStorage.GetAsync<UserDTO>("User");
            if(userSession.Success && userSession.Value != null)
            {
                var user = userSession.Value;
                var claims = new[] {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                _currentUser = new ClaimsPrincipal(identity);
            } else {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
            return await Task.FromResult(new AuthenticationState(_currentUser));
        }




    }
}
