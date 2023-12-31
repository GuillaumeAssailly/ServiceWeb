﻿using Front.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;


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
    }
}

