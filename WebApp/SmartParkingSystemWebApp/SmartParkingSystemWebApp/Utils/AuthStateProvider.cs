using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartParkingSystemWebApp.Utils
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly AuthenticationState anonymous;
        private readonly HttpClient _client;
        public JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();


        public AuthStateProvider(ISessionStorageService sessionStorageService, HttpClient client)
        {
            _sessionStorageService = sessionStorageService;
            anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            _client = client;
        }


        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _sessionStorageService.GetItemAsStringAsync("accessToken");

            if (String.IsNullOrEmpty(token))
                return anonymous;

            token = token.Remove(0, 1);
            token = token.Substring(0, token.Length - 1);
            var cp = new ClaimsPrincipal(GetClaims(token));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);



            return new AuthenticationState(cp);
        }
        public void NotifyUserLogin(string token)
        {
            var cp = new ClaimsPrincipal(GetClaims(token));
            var authState = Task.FromResult(new AuthenticationState(cp));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private ClaimsIdentity GetClaims(string token)
        {
            string KullaniciAdi = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "KullaniciAdi").Value;
            string Ad = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "Ad").Value;
            string Soyad = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "Soyad").Value;
            string Id = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "Id").Value;
            string IsAdmin = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "IsAdmin").Value;
            string BolgeId = tokenHandler.ReadJwtToken(token).Claims.First(claim => claim.Type == "BolgeId").Value;


            var claims = new ClaimsIdentity(new[] {
                new Claim("KullaniciAdi", KullaniciAdi, ClaimValueTypes.String),
                new Claim("Ad", Ad, ClaimValueTypes.String),
                new Claim("Soyad", Soyad , ClaimValueTypes.String),
                new Claim("Id", Id, ClaimValueTypes.String),
                new Claim("IsAdmin", IsAdmin, ClaimValueTypes.Boolean),
                new Claim("BolgeId", BolgeId, ClaimValueTypes.String)
            }, "jwtAuthType");


            return claims;
        }

    }
}
