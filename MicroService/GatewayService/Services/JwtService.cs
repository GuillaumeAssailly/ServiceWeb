using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GatewayService.Services
{
    public class JwtService
    {
        private IHttpContextAccessor _contextAccessor;
        private TokenValidationParameters _tokenValidationParameters;
        public JwtService(IOptions<TokenValidationParameters> tokenValidationParameters, IHttpContextAccessor httpContextAccessor) 
        { 
            _contextAccessor = httpContextAccessor;
            _tokenValidationParameters = tokenValidationParameters.Value;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
        }

        public bool isLogged(out string errorMessage)
        {
            errorMessage = "";
            var httpContext = _contextAccessor.HttpContext;
            var tokenWithQuotes = httpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(tokenWithQuotes))
            {
                errorMessage = "Authorization token is missing.";
                return false;
            }

            //var principal = ValidateToken(token);
            try
            {
                var token = tokenWithQuotes.Trim('"'); // Remove surrounding quotes from the token
                var principal = ValidateToken(token);
                return true; // User is authorized
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating token: {ex.Message}";
                return false;
            }
            //return true; // User is authorized
        }

    }
}
